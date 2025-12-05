// Konfigurasjon
const CONFIG = {
    DEFAULT_VIEW: [59.9139, 10.7522],
    DEFAULT_ZOOM: 7,
    CURRENT_LOCATION_ZOOM: 15,
    TOLERANCE: 0.0001, // ~10 meter
    STATUS_COLORS: {
        1: '#ff9800', // Orange - Under behandling
        2: '#4caf50', // Green - Godkjent
        3: '#f44336'  // Red - Avslått
    },
    STATUS_TEXTS: {
        1: 'Under behandling',
        2: 'Godkjent',
        3: 'Avslått'
    },
    DRAW_COLOR: '#ff6b00',
    DRAW_WEIGHT: 4,
    GEOLOCATION_OPTIONS: {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0
    }
};

// API-endepunkter
const API = {
    FORM_PARTIAL: '/Obstacle/DataFormPartial',
    GET_PENDING: '/Map/GetPendingObstacles',
    SUBMIT: '/Obstacle/SubmitObstacle',
    QUICK_SAVE: '/Obstacle/QuickSaveObstacle'
};

// GeoJSON-hjelper
class GeoJsonHelper {
    static parse(geoJsonValue) {
        try {
            if (typeof geoJsonValue === 'string') {
                return JSON.parse(geoJsonValue);
            } else {
                return geoJsonValue;
            }
        } catch (err) {
            console.error("Feil ved parsing av GeoJSON:", err);
            return null;
        }
    }

    static extractGeometry(geoJsonValue) {
        const parsed = this.parse(geoJsonValue);
        if (!parsed) return null;
        if (parsed.geometry) {
            return parsed.geometry;
        } else {
            return parsed;
        }
    }

    static extractCoordinates(geoJsonValue) {
        const geometry = this.extractGeometry(geoJsonValue);
        if (!geometry || geometry.type !== "Point" || !geometry.coordinates || geometry.coordinates.length < 2) {
            return null;
        }
        return {
            lat: geometry.coordinates[1],
            lng: geometry.coordinates[0]
        };
    }

    static createPoint(lat, lng) {
        return {
            type: "Feature",
            geometry: {
                type: "Point",
                coordinates: [lng, lat]
            },
            properties: {}
        };
    }

    static toCoordinatesArray(geoJsonValue) {
        const geometry = this.extractGeometry(geoJsonValue);
        if (!geometry || !geometry.coordinates) return null;
        
        if (geometry.type === "Point") {
            return geometry.coordinates;
        }
        return geometry.coordinates;
    }
}

// Status-hjelper
class StatusHelper {
    static getText(status) {
        if (CONFIG.STATUS_TEXTS[status]) {
            return CONFIG.STATUS_TEXTS[status];
        } else {
            return 'Ukjent';
        }
    }

    static getColor(status) {
        if (CONFIG.STATUS_COLORS[status]) {
            return CONFIG.STATUS_COLORS[status];
        } else {
            return '#9e9e9e';
        }
    }
}

class MarkerCreator {
    static createStatusIcon(statusColor) {
        return L.divIcon({
            className: 'reported-obstacle-marker',
            html: `<div style="background-color: ${statusColor}; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>`,
            iconSize: [20, 20],
            iconAnchor: [10, 10]
        });
    }

    static createPopupContent(obstacle) {
        const statusColor = StatusHelper.getColor(obstacle.status);
        const statusText = StatusHelper.getText(obstacle.status);
        
        let obstacleName;
        if (obstacle.name) {
            obstacleName = obstacle.name;
        } else {
            obstacleName = 'Rapportert hindring';
        }
        
        let obstacleHeight;
        if (obstacle.height) {
            obstacleHeight = obstacle.height;
        } else {
            obstacleHeight = 'N/A';
        }
        
        return `
            <strong>${obstacleName}</strong><br>
            Høyde: ${obstacleHeight}m<br>
            <small>Status: <span style="color: ${statusColor}; font-weight: bold;">${statusText}</span></small>
        `;
    }

    static createPointMarker(obstacle, lat, lng, reportedMarkers) {
        const statusColor = StatusHelper.getColor(obstacle.status);
        const popupContent = this.createPopupContent(obstacle);
        const icon = this.createStatusIcon(statusColor);
        const marker = L.marker([lat, lng], { icon }).bindPopup(popupContent);
        reportedMarkers.addLayer(marker);
    }

    static createLineStringMarker(obstacle, coordinates, reportedMarkers) {
        const statusColor = StatusHelper.getColor(obstacle.status);
        const popupContent = this.createPopupContent(obstacle);
        const icon = this.createStatusIcon(statusColor);
        
        // Konverter koordinater fra [lng, lat] til [lat, lng] for Leaflet
        const latlngs = coordinates.map(coord => [coord[1], coord[0]]);
        
        // Opprett polyline
        const polyline = L.polyline(latlngs, {
            color: statusColor,
            weight: 4,
            opacity: 0.8
        }).bindPopup(popupContent);
        
        reportedMarkers.addLayer(polyline);
        
        // Legg til markører ved hvert hjørne/punkt
        latlngs.forEach((latlng, index) => {
            const cornerMarker = L.marker(latlng, { icon })
                .bindPopup(`${popupContent}<br><small>Punkt ${index + 1} av ${latlngs.length}</small>`);
            reportedMarkers.addLayer(cornerMarker);
        });
    }
}


class ObstacleMap {
    constructor(mapElementId, reportedObstacles) {
        this.map = L.map(mapElementId).setView(CONFIG.DEFAULT_VIEW, CONFIG.DEFAULT_ZOOM);
        this.drawnItems = new L.FeatureGroup();
        this.reportedMarkers = new L.FeatureGroup();
        this.currentDrawHandler = null;
        this.lastGeoJsonString = "";
        this.currentLocationMarker = null;
        if (reportedObstacles) {
            this.pendingObstacles = reportedObstacles.filter(function(obstacle) {
                return obstacle.status === 1;
            });
        } else {
            this.pendingObstacles = [];
        }
        
        this.initializeMap();
        this.setupDrawingTools();
        this.addReportedObstacles(reportedObstacles);
    }

    initializeMap() {
        // Legg til OSM tile layer
        const osm = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        });
        osm.addTo(this.map);
        
        // Legg til feature groups
        this.map.addLayer(this.drawnItems);
        this.map.addLayer(this.reportedMarkers);
    }

    addReportedObstacles(obstacles) {
        if (!obstacles || obstacles.length === 0) return;

        obstacles.forEach(obstacle => {
            if (!obstacle.geometryGeoJson) return;

            try {
                const geometry = GeoJsonHelper.extractGeometry(obstacle.geometryGeoJson);
                if (!geometry || !geometry.coordinates) return;

                const geometryType = geometry.type;
                const coordinates = geometry.coordinates;

                if (geometryType === "Point" && coordinates.length >= 2) {
                    const lat = coordinates[1];
                    const lng = coordinates[0];
                    MarkerCreator.createPointMarker(obstacle, lat, lng, this.reportedMarkers);
                } else if (geometryType === "LineString" && Array.isArray(coordinates) && coordinates.length > 0) {
                    MarkerCreator.createLineStringMarker(obstacle, coordinates, this.reportedMarkers);
                }
            } catch (err) {
                console.error("Feil ved parsing av hindring GeoJSON:", err, obstacle);
            }
        });
    }

    setupDrawingTools() {
        // Punkt-knapp
        document.getElementById('draw-point-btn').addEventListener('click', () => {
            this.stopDrawing();
            this.currentDrawHandler = new L.Draw.Marker(this.map, {
                icon: new L.Icon.Default()
            });
            this.currentDrawHandler.enable();
            this.updateButtonStates('point');
        });

        // Linje-knapp
        document.getElementById('draw-line-btn').addEventListener('click', () => {
            this.stopDrawing();
            this.currentDrawHandler = new L.Draw.Polyline(this.map, {
                allowIntersection: false,
                shapeOptions: {
                    color: CONFIG.DRAW_COLOR,
                    weight: CONFIG.DRAW_WEIGHT
                }
            });
            this.currentDrawHandler.enable();
            this.updateButtonStates('line');
        });

        // Tegning opprettet-hendelse
        this.map.on(L.Draw.Event.CREATED, (e) => {
            this.handleDrawingCreated(e);
        });
    }

    stopDrawing() {
        if (this.currentDrawHandler) {
            this.map.removeLayer(this.currentDrawHandler);
            this.currentDrawHandler = null;
        }
    }

    updateButtonStates(activeType) {
        const pointBtn = document.getElementById('draw-point-btn');
        const lineBtn = document.getElementById('draw-line-btn');
        
        pointBtn.classList.toggle('active', activeType === 'point');
        lineBtn.classList.toggle('active', activeType === 'line');
    }

    handleDrawingCreated(e) {
        this.stopDrawing();
        this.updateButtonStates(null);

        const layer = e.layer;
        this.drawnItems.addLayer(layer);

        const geoJsonData = layer.toGeoJSON();
        const coordinatesOnly = geoJsonData.geometry.coordinates;
        this.lastGeoJsonString = JSON.stringify(geoJsonData);

        this.updateGeoJsonInputs(geoJsonData, coordinatesOnly);
        this.hideLocationButton();
        this.openObstacleForm();
    }

    updateGeoJsonInputs(geoJsonData, coordinatesOnly) {
        const geoInput = document.getElementById('GeometryGeoJsonMap');
        const coordsInput = document.getElementById('GeometryGeoJsonCoordinates');

        if (geoInput) {
            if (typeof geoJsonData === 'string') {
                geoInput.value = geoJsonData;
            } else {
                geoInput.value = JSON.stringify(geoJsonData);
            }
        }
        if (coordsInput) {
            coordsInput.value = JSON.stringify(coordinatesOnly);
        }
    }

    updateGeoJsonFromCoordinates(lat, lng) {
        const geoJsonData = GeoJsonHelper.createPoint(lat, lng);
        const coordinatesOnly = [lng, lat];
        this.lastGeoJsonString = JSON.stringify(geoJsonData);
        this.updateGeoJsonInputs(geoJsonData, coordinatesOnly);
    }

    checkForPendingObstacle(lat, lng) {
        if (!this.pendingObstacles || this.pendingObstacles.length === 0) {
            return null;
        }

        for (const obstacle of this.pendingObstacles) {
            if (!obstacle.geometryGeoJson) continue;

            try {
                const geometry = GeoJsonHelper.extractGeometry(obstacle.geometryGeoJson);
                if (!geometry || geometry.type !== "Point") continue;

                const coords = geometry.coordinates;
                const obstacleLat = coords[1];
                const obstacleLng = coords[0];

                if (Math.abs(obstacleLat - lat) < CONFIG.TOLERANCE &&
                    Math.abs(obstacleLng - lng) < CONFIG.TOLERANCE) {
                    return obstacle;
                }
            } catch (err) {
                console.error("Feil ved parsing av hindring GeoJSON:", err);
            }
        }
        return null;
    }

    async fetchPendingObstacles() {
        try {
            const response = await fetch(API.GET_PENDING);
            if (!response.ok) throw new Error('Nettverksresponsen var ikke OK');
            return await response.json();
        } catch (error) {
            console.error("Feil ved henting av hindringer under behandling:", error);
            return this.pendingObstacles; // Tilbakefall
        }
    }

    showPendingObstacleWarning(formElem, existingObstacle) {
        const warningDiv = formElem.querySelector('#pending-obstacle-warning');
        if (!warningDiv) return;

        warningDiv.style.display = 'block';
        const warningText = warningDiv.querySelector('.warning-text');
        if (warningText) {
            let obstacleName;
            if (existingObstacle.name) {
                obstacleName = existingObstacle.name;
            } else {
                obstacleName = 'Rapport under behandling';
            }
            warningText.textContent = 
                `Det er allerede en ubehandlet rapport i denne koordinaten (${obstacleName}). Du er fortsatt velkommen til å rapportere.`;
        }
    }

    async checkAndShowConflictWarning(formElem, coords) {
        if (!coords) return;

        this.pendingObstacles = await this.fetchPendingObstacles();
        const existingObstacle = this.checkForPendingObstacle(coords.lat, coords.lng);
        
        if (existingObstacle) {
            this.showPendingObstacleWarning(formElem, existingObstacle);
        }
    }

    closeObstacleForm() {
        this.drawnItems.clearLayers();
        this.currentLocationMarker = null;

        const geoJsonInput = document.getElementById("GeometryGeoJsonMap");
        const coordinatesInput = document.getElementById("GeometryGeoJsonCoordinates");
        if (geoJsonInput) geoJsonInput.value = "";
        if (coordinatesInput) coordinatesInput.value = "";

        const formElem = document.getElementById("obstacle-form");
        if (formElem) formElem.style.display = "none";

        const formContainer = document.getElementById("form-container");
        if (formContainer) formContainer.innerHTML = "";

        const overlay = document.querySelector(".obstacle-form-overlay");
        if (overlay) overlay.style.display = "none";

        this.showLocationButton();
    }

    getGeoJsonValue() {
        const geoJsonInput = document.getElementById("GeometryGeoJsonMap");
        const coordinatesInput = document.getElementById("GeometryGeoJsonCoordinates");
        
        let geoJsonValue = "";
        if (geoJsonInput && geoJsonInput.value) {
            geoJsonValue = geoJsonInput.value;
        }
        
        let geoJsonCoordinates = "";
        if (coordinatesInput && coordinatesInput.value) {
            geoJsonCoordinates = coordinatesInput.value;
        }

        // Hvis ingen verdi i hidden input, prøv lastGeoJsonString
        if (!geoJsonValue && this.lastGeoJsonString) {
            geoJsonValue = this.lastGeoJsonString;
            const coords = GeoJsonHelper.toCoordinatesArray(this.lastGeoJsonString);
            if (coords) {
                geoJsonCoordinates = JSON.stringify(coords);
            }
        }

        // Hvis fortsatt ingen verdi, sjekk drawn items
        if (!geoJsonValue && this.drawnItems.getLayers().length > 0) {
            const lastLayer = this.drawnItems.getLayers()[this.drawnItems.getLayers().length - 1];
            const geoJsonData = lastLayer.toGeoJSON();
            geoJsonValue = JSON.stringify(geoJsonData);
            geoJsonCoordinates = JSON.stringify(geoJsonData.geometry.coordinates);
            
            // Oppdater hidden inputs
            if (geoJsonInput) geoJsonInput.value = geoJsonValue;
            if (coordinatesInput) coordinatesInput.value = geoJsonCoordinates;
        }

        return { geoJsonValue, geoJsonCoordinates };
    }

    populateFormFields(formElem, geoJsonValue, geoJsonCoordinates) {
        let formGeoInput = formElem.querySelector('input[name="ViewGeometryGeoJson"]');
        if (!formGeoInput) {
            formGeoInput = formElem.querySelector('#GeometryGeoJson');
        }
        const formGeoInputCoordinates = formElem.querySelector('input[name="GeometryGeoCoordinates"]');

        if (formGeoInput) {
            formGeoInput.value = geoJsonValue;
        } else {
            console.error("Kunne ikke finne ViewGeometryGeoJson-input i skjemaet");
        }

        if (formGeoInputCoordinates) {
            formGeoInputCoordinates.value = geoJsonCoordinates;
        } else {
            console.error("Kunne ikke finne input[name='GeometryGeoCoordinates'] i skjemaet");
        }
    }

    setupCancelButton(formElem) {
        const cancelButton = formElem.querySelector('#cancel-obstacle-button');
        if (cancelButton) {
            cancelButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.closeObstacleForm();
            });
        }
    }

    async loadFormPartial() {
        try {
            const response = await fetch(API.FORM_PARTIAL);
            if (!response.ok) throw new Error('Nettverksresponsen var ikke OK');
            return await response.text();
        } catch (error) {
            console.error("Feil ved lasting av skjema:", error);
            throw error;
        }
    }

    async openObstacleForm() {
        const { geoJsonValue, geoJsonCoordinates } = this.getGeoJsonValue();

        if (!geoJsonValue) {
            alert("Vennligst klikk på kartet for å velge hindringens plassering før registrering.");
            return;
        }

        try {
            const html = await this.loadFormPartial();
            const formContainer = document.getElementById("form-container");
            formContainer.innerHTML = html;

            const formElem = document.getElementById("obstacle-form");
            if (!formElem) {
                console.error("Kunne ikke finne hindringsskjema-element");
                return;
            }

            this.setupCancelButton(formElem);
            this.populateFormFields(formElem, geoJsonValue, geoJsonCoordinates);
            formElem.style.display = "block";

            // Sjekk for konflikter
            const coords = GeoJsonHelper.extractCoordinates(geoJsonValue);
            await this.checkAndShowConflictWarning(formElem, coords);

            // Oppsett av validering
            if (typeof $.validator !== 'undefined') {
                $.validator.unobtrusive.parse(formElem);
            }

            // Oppsett av skjema innsendings-håndterer
            formElem.addEventListener("submit", (e) => this.handleFormSubmit(e));
        } catch (error) {
            console.error("Feil ved åpning av skjema:", error);
            alert("En feil oppstod ved lasting av skjemaet. Vennligst prøv igjen.");
        }
    }

    async handleFormSubmit(e) {
        e.preventDefault();

        const currentForm = e.target;
        const formData = new FormData(currentForm);
        
        let submitButton;
        if (e.submitter) {
            submitButton = e.submitter;
        } else {
            submitButton = document.activeElement;
        }
        let actionUrl = API.SUBMIT;
        
        if (submitButton && submitButton.type === 'submit') {
            if (submitButton.hasAttribute('formaction')) {
                actionUrl = submitButton.getAttribute('formaction');
            } else if (submitButton.id === 'obstacle-quick-save-button') {
                actionUrl = API.QUICK_SAVE;
            }
        }

        try {
            const response = await fetch(actionUrl, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            const contentType = response.headers.get("content-type");
            let data;

            if (contentType && contentType.includes("application/json")) {
                data = await response.json();
            } else {
                const html = await response.text();
                data = { html };
            }

            if (data.redirectUrl) {
                window.location.href = data.redirectUrl;
            } else if (data.html) {
                await this.handleFormValidationError(data.html);
            }
        } catch (error) {
            console.error("Feil ved innsending av skjema:", error);
            alert("En feil oppstod ved innsending av skjemaet. Vennligst prøv igjen.");
        }
    }

    async handleFormValidationError(html) {
        const formContainer = document.getElementById("form-container");
        formContainer.innerHTML = html;

        const updatedForm = document.getElementById("obstacle-form");
        if (!updatedForm) return;

        this.setupCancelButton(updatedForm);

        // Kopier koordinater tilbake
        const geoJsonInput = document.getElementById("GeometryGeoJsonMap");
        const coordinatesInput = document.getElementById("GeometryGeoJsonCoordinates");
        
        let geoJsonValue = "";
        if (geoJsonInput && geoJsonInput.value) {
            geoJsonValue = geoJsonInput.value;
        }
        
        let geoJsonCoordinates = "";
        if (coordinatesInput && coordinatesInput.value) {
            geoJsonCoordinates = coordinatesInput.value;
        }
        
        this.populateFormFields(updatedForm, geoJsonValue, geoJsonCoordinates);

        updatedForm.style.display = "block";

        // Sjekk for konflikter igjen
        let geoJsonValueForCoords = "";
        if (geoJsonInput && geoJsonInput.value) {
            geoJsonValueForCoords = geoJsonInput.value;
        }
        const coords = GeoJsonHelper.extractCoordinates(geoJsonValueForCoords);
        await this.checkAndShowConflictWarning(updatedForm, coords);

        // Oppsett av validering og innsendings-håndterer på nytt
        if (typeof $.validator !== 'undefined') {
            $.validator.unobtrusive.parse(updatedForm);
        }
        updatedForm.addEventListener("submit", (e) => this.handleFormSubmit(e));
    }

    setupCurrentLocationButton() {
        const button = document.getElementById("use-current-location");
        if (!button) return;

        button.addEventListener("click", () => this.handleCurrentLocation());
    }

    async handleCurrentLocation() {
        if (!navigator.geolocation) {
            alert("Geolokasjon støttes ikke av nettleseren din.");
            return;
        }

        const button = document.getElementById("use-current-location");
        const originalText = button.textContent;
        button.disabled = true;
        button.textContent = "Henter posisjon...";

        try {
            const position = await this.getCurrentPosition();
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;

            this.drawnItems.clearLayers();
            this.currentLocationMarker = null;

            this.currentLocationMarker = L.marker([lat, lng]);
            this.drawnItems.addLayer(this.currentLocationMarker);
            this.map.setView([lat, lng], CONFIG.CURRENT_LOCATION_ZOOM);

            this.updateGeoJsonFromCoordinates(lat, lng);
            this.hideLocationButton();
            await this.openObstacleForm();
        } catch (error) {
            this.handleGeolocationError(error);
        } finally {
            button.disabled = false;
            button.textContent = originalText;
        }
    }

    getCurrentPosition() {
        return new Promise((resolve, reject) => {
            navigator.geolocation.getCurrentPosition(
                resolve,
                reject,
                CONFIG.GEOLOCATION_OPTIONS
            );
        });
    }

    handleGeolocationError(error) {
        let errorMessage = "Kunne ikke hente din posisjon. ";
        switch (error.code) {
            case error.PERMISSION_DENIED:
                errorMessage += "Posisjonstilgang nektet av bruker.";
                break;
            case error.POSITION_UNAVAILABLE:
                errorMessage += "Posisjonsinformasjon utilgjengelig.";
                break;
            case error.TIMEOUT:
                errorMessage += "Posisjonsforespørsel utløpt.";
                break;
            default:
                errorMessage += "En ukjent feil oppstod.";
                break;
        }
        alert(errorMessage);
    }

    hideLocationButton() {
        const buttonContainer = document.querySelector('.location-button-container');
        if (buttonContainer) {
            buttonContainer.style.display = 'none';
        }
    }

    showLocationButton() {
        const buttonContainer = document.querySelector('.location-button-container');
        if (buttonContainer) {
            buttonContainer.style.display = 'block';
        }
    }
}

// Initialisering
document.addEventListener("DOMContentLoaded", function () {
    // Initialiser kartet med rapporterte hindringer fra server
    if (typeof reportedObstacles !== 'undefined') {
        window.obstacleMap = new ObstacleMap('map', reportedObstacles);
        window.obstacleMap.setupCurrentLocationButton();
    } else {
        console.error("reportedObstacles er ikke definert");
    }
});

