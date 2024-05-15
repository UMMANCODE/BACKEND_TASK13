$(document).ready(function () {
    $("#imgInput").change(function (e) {
        var img = document.getElementById('previewImg');
        img.src = URL.createObjectURL(e.target.files[0]);
        img.onload = function () {
            URL.revokeObjectURL(img.src);
        };
    });

    document.querySelectorAll(".deleted").forEach(item => {
        item.addEventListener("click", function (e) {
            e.preventDefault();
            let url = this.getAttribute("href");
            Swal.fire({
                title: "Are you sure?",
                text: "You won't be able to revert this!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "Yes, delete it!"
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch(url, {
                        method: 'DELETE'
                    })
                        .then(response => {
                            if (response.ok) {
                                Swal.fire({
                                    title: "Deleted!",
                                    text: "Your file has been deleted.",
                                    icon: "success"
                                }).then(() => {
                                    location.reload();
                                });
                            } else {
                                Swal.fire({
                                    title: "Error!",
                                    text: "Failed to delete the item.",
                                    icon: "error"
                                });
                            }
                        })
                        .catch(error => {
                            Swal.fire({
                                title: "Error!",
                                text: "An error occurred while deleting the item: " + error,
                                icon: "error"
                            });
                        });
                }
            });
        });
    });
});
