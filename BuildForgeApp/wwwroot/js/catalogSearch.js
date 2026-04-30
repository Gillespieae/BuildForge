document.addEventListener("DOMContentLoaded", () => {
    // get the search input field
    const searchInput = document.getElementById("catalogSearch");

    // get all rows from the component table
    const tableRows = document.querySelectorAll("#componentTable tbody tr");

    // stop if input or table rows don't exist (prevents errors)
    if (!searchInput || tableRows.length === 0) {
        return;
    }

    // run this every time the user types in the search box
    searchInput.addEventListener("input", () => {
        const searchText = searchInput.value.toLowerCase(); // normalize for case-insensitive search

        tableRows.forEach(row => {
            const rowText = row.innerText.toLowerCase(); // get all text in the row

            // show row if it matches search, otherwise hide it
            if (rowText.includes(searchText)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    });
});