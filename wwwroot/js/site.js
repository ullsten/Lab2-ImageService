//script for checkbox to work. May need this sometimes.
//*********************************************************** */
    $(document).ready(function () {
        $('form').on('change', ':checkbox', function () {
            if (this.checked) {
                $(this).val(true);
            }
            else {
                $(this).val(false);
            }
        });
                });
//*********************************************************** */

$(document).ready(function () {
    // Open modal popup when the button is clicked
    $("#openModal").click(function () {
        $("#modalPopup").modal("show");
    });

    // Close modal popup when the close button is clicked
    $("#closeModal").click(function () {
        $("#modalPopup").modal("hide");
    });

    // Update the image and download link when an option is selected from the dropdown lists
    $("#objectsDropdown, #thumbnailsDropdown, #uploadedDropdown").change(function () {
        var selectedFolder = $(this).val(); // Get the selected folder and image name
        $("#selectedImage").attr("src", selectedFolder).show();

        // Update the download link
        $("#downloadLink").attr("href", selectedFolder).show();
    });

    // Add a click event for the download button
    $("#downloadButton").click(function () {
        // Trigger the click event on the download link
        $("#downloadLink")[0].click();
    });
});


//*********************************************************** */


