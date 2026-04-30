document.addEventListener("DOMContentLoaded", () => {
    // button that triggers compatibility check
    const button = document.getElementById("checkCompatibilityBtn");

    // div where results (success/warnings) will be displayed
    const resultBox = document.getElementById("compatibilityResult");

    // stop if required elements aren't found (prevents runtime errors)
    if (!button || !resultBox) {
        return;
    }

    // runs when user clicks the "Check Compatibility" button
    button.addEventListener("click", async () => {
        // get buildId from data attribute on the button
        const buildId = button.dataset.buildId;

        // calls the API endpoint
        const response = await fetch(`/api/BuildCompatibilityApi/${buildId}`);

        // handle failed request
        if (!response.ok) {
            resultBox.innerHTML = `<div class="alert alert-danger">Could not check compatibility.</div>`;
            return;
        }

        // convert response to JSON
        const result = await response.json();

        // if no warnings then build is compatible
        if (result.isCompatible) {
            resultBox.innerHTML = `<div class="alert alert-success">This build is compatible.</div>`;
        } else {
            // convert warnings array into list items
            const warningList = result.warnings.map(w => `<li>${w}</li>`).join("");

            // display warnings in a Bootstrap alert
            resultBox.innerHTML = `
                <div class="alert alert-warning">
                    <strong>Compatibility Issues:</strong>
                    <ul class="mb-0">${warningList}</ul>
                </div>
            `;
        }
    });
});