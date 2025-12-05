
// Adaptiv sjekk av passordkrav for admin CreateUser-skjema.
// Oppdaterer kravene mens brukeren skriver og når brukeren forlater passordfeltet.
document.addEventListener('DOMContentLoaded', function () {
    var pwd = document.querySelector('input[name="Password"], #Password');
    var reqContainer = document.getElementById('pw-reqs-container');
    var reqList = document.getElementById('pw-reqs');

    if (!pwd || !reqContainer || !reqList) return;

    var policy = {};
    try {
        policy = JSON.parse(reqContainer.dataset.policy || '{}');
    } catch (e) {
        policy = {};
    }

    // Hjelpefunksjon for å telle antall unike tegn
    function countUniqueChars(s) {
        var set = new Set();
        for (var i = 0; i < s.length; i++) set.add(s[i]);
        return set.size;
    }

    function checkPassword(p) {
        var checks = [];

       // Påkrevd lengde
        var minLen = policy.RequiredLength || 0;
        checks.push({
            ok: p.length >= minLen,
            message: "Minimum lengde: " + minLen + " tegn"
        });

        if (policy.RequireDigit) {
            checks.push({
                ok: /[0-9]/.test(p),
                message: "Minst ett siffer (0-9)"
            });
        }
        if (policy.RequireLowercase) {
            checks.push({
                ok: /[a-z]/.test(p),
                message: "Minst en liten bokstav (a-z)"
            });
        }
        if (policy.RequireUppercase) {
            checks.push({
                ok: /[A-Z]/.test(p),
                message: "Minst en stor bokstav (A-Z)"
            });
        }
        if (policy.RequireNonAlphanumeric) {
            checks.push({
                ok: /[^a-zA-Z0-9]/.test(p),
                message: "Minst ett ikke-alfanumerisk tegn (f.eks. !, @, #)"
            });
        }
        if (policy.RequiredUniqueChars && policy.RequiredUniqueChars > 1) {
            checks.push({
                ok: countUniqueChars(p) >= policy.RequiredUniqueChars,
                message: "Minst " + policy.RequiredUniqueChars + " unike tegn"
            });
        }

        return checks;
    }

    function renderChecks(checks) {
        var items = reqList.querySelectorAll('.pw-req-item');
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            var icon = item.querySelector('.pw-req-icon');
            var text = item.querySelector('.pw-req-text');
            var check = checks[i];
            if (!check) continue;
            if (check.ok) {
                item.classList.remove('text-muted');
                item.classList.add('text-success');
                if (icon) icon.textContent = "✔";
            } else {
                item.classList.remove('text-success');
                item.classList.add('text-muted');
                if (icon) icon.textContent = "◻";
            }
            if (text) text.textContent = check.message;
        }
    }

    // Første oppdatering (tomt passord)
    renderChecks(checkPassword(pwd.value || ''));

    // Live oppdatering mens brukeren skriver
    pwd.addEventListener('input', function (e) {
        renderChecks(checkPassword(e.target.value || ''));
    });

    // Når brukeren forlater passordfeltet (tabber videre), oppdater UI
    pwd.addEventListener('blur', function (e) {
        renderChecks(checkPassword(e.target.value || ''));
    });

    // Hvis brukeren fokuserer på neste felt etter passord, oppdater også.
    // Lytter på focusin på skjemaet og oppdaterer hvis fokus flyttes fra passordfeltet.
    var form = pwd.closest('form');
    if (form) {
        form.addEventListener('focusin', function (e) {
            // Hvis fokus har flyttet seg bort fra passordfeltet, oppdater kravene
            if (document.activeElement !== pwd) {
                renderChecks(checkPassword(pwd.value || ''));
            }
        });
    }
});
