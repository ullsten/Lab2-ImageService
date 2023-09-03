document.addEventListener("DOMContentLoaded", function () {
    const checkbox = document.getElementById("addThumbnail");

    checkbox.addEventListener("change", function () {
        if (checkbox.checked) {
            console.log("Checkbox is checked");
        } else {
            console.log("Checkbox is unchecked");
        }
    });
});