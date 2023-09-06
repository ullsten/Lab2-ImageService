
    // Get the modal element and close button
    var modal = document.getElementById('myModal');
    var modalImage = document.getElementById('modalImage');
    var closeBtn = document.getElementsByClassName('close')[0];

    // Function to open the modal and set the image source
    function openModal(imageSrc) {
        modal.style.display = 'block';
        modalImage.src = imageSrc;
    }

    // Function to close the modal
    function closeModal() {
        modal.style.display = 'none';
    }

    // Attach click event handlers to the images
    var images = document.querySelectorAll('.img-thumbnail');

    images.forEach(function (image) {
        image.addEventListener('click', function () {
            openModal(this.src);
        });
    });

    // Attach click event handler to close button
    closeBtn.addEventListener('click', closeModal);

    // Close the modal when the user clicks outside of it
    window.addEventListener('click', function (event) {
        if (event.target == modal) {
            closeModal();
        }
    });




    // Function to scroll to the top of the page
    function scrollToTop() {
        $('html, body').animate({ scrollTop: 0 }, 200); // 1000 milliseconds (1 second) for smooth scrolling
        // Hide other image categories
        $(".thumbnails").hide();
        $(".object-images").hide();
        $(".uploaded-images").hide();
    }


//JavaScript / jQuery code to toggle visibility

        $(document).ready(function () {
            // Function to scroll to a specific div
            function scrollToDiv(divId) {
                $('html, body').animate({
                    scrollTop: $(divId).offset().top
                }, 1000); // 1000 milliseconds (1 second) for smooth scrolling
            }

        // Show Uploaded Images and scroll to the Uploaded Images div
        $("#showUploadedImages").click(function () {
            // Hide other image categories
            $(".thumbnails").hide();
        $(".object-images").hide();

        // Toggle the visibility of the Uploaded Images category
        $(".uploaded-images").toggle();

        // Scroll to the Uploaded Images div
        scrollToDiv("#uploadedImagesDiv");
        });

        // Show Thumbnails and scroll to the Thumbnails div
        $("#showThumbnails").click(function () {
            // Hide other image categories
            $(".uploaded-images").hide();
        $(".object-images").hide();

        // Toggle the visibility of the Thumbnails category
        $(".thumbnails").toggle();

        // Scroll to the Thumbnails div
        scrollToDiv("#thumbnailsDiv");
        });

        // Show Objects Images and scroll to the Objects Images div
        $("#showObjectImages").click(function () {
            // Hide other image categories
            $(".uploaded-images").hide();
        $(".thumbnails").hide();

        // Toggle the visibility of the Object Images category
        $(".object-images").toggle();

        // Scroll to the Object Images div
        scrollToDiv("#objectImagesDiv");
        });
    });
