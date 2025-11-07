// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Adaptive password requirement checker for admin CreateUser form.
// Updates requirements as user types and when the user leaves the password field.

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

    // Helper to check unique chars count
    function countUniqueChars(s) {
        var set = new Set();
        for (var i = 0; i < s.length; i++) set.add(s[i]);
        return set.size;
    }

    function checkPassword(p) {
        var checks = [];

        // Required length
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

    // Initial render (empty password)
    renderChecks(checkPassword(pwd.value || ''));

    // Live feedback while typing
    pwd.addEventListener('input', function (e) {
        renderChecks(checkPassword(e.target.value || ''));
    });

    // When user leaves the password field (tabs to next), also update UI
    pwd.addEventListener('blur', function (e) {
        renderChecks(checkPassword(e.target.value || ''));
    });

    // If user focuses next control after password, update as well.
    // Listen for focusin on the form and if coming from password, update.
    var form = pwd.closest('form');
    if (form) {
        form.addEventListener('focusin', function (e) {
            // If focus moved away from password, refresh checks
            if (document.activeElement !== pwd) {
                renderChecks(checkPassword(pwd.value || ''));
            }
        });
    }
});
