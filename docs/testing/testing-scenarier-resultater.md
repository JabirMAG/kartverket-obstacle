# Dokumentasjon Testing, scenarier og resultater

Dokumentasjoenen viser enhetstester for datamodellen ObstacLeData og kontrolleren HomeController i applikasjoen. 

## Testscenarier 

| ID | Hva testes | Input | Forventet resultat |
|----|-------------|--------|--------------------|
| T01 | Gyldig modell aksepteres            | Name = "Tre" , Height=15       | skal være gyldig (ingen valideringsfeil)                   |
| T02 | påkrevd felt  mangler           |  Name="" og Description=""      |   skal feile og vise feilmelding   "Field is required"                |
| T03 | Høyde utenfor gyldig område (0-200)            |  Height = 500      | Skal feile med feilmelding om høydeområde                 |
| T04 | HomeController.Dataform setter Gretting | Kall Data Form() i Homecontroller | ViewBag.Greeting skal innholde enten "Good Morning!" , "Good Afternoon!" eller "Good Evening!" |

# test resultat

|ID  | Faktisk resultat | Status |
|----|-------------------|--------|
| T01 | Modellen var gyldig med korrekte verdier | PASS |
| t02 | Feilmelding "Field is required" kom opp ved tomme felt | PASS |
| T03 |Feilmelding om høydeomårde kom som forventet  | pass  |
| T04 | Greeting ble satt som forventet (en av tre mulige verider) | PASS |


Denne dokumentasjonen viser enhetstester for både datamodellen *ObstacleData* og kontrolleren *HomeController* i applikasjonen. Testene verifiserer at valideringsregler og logikk fungerer som forventet.
