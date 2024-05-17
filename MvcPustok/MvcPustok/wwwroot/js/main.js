
$(document).ready(function () {
  $(".book-modal").click(function (e) {
    e.preventDefault();
    let url = this.getAttribute("href");

    fetch(url)
      .then(response => response.text())
      .then(data => {
        $("#quickModal .modal-dialog").html(data)
      })

    $("#quickModal").modal('show');
  })

  $(".order-detail-view").click(function (e) {
    e.preventDefault();
    let url = this.getAttribute("href");

    fetch(url)
      .then(response => response.text())
      .then(data => {
        $("#orderDetailModal .modal-dialog").html(data)
      })

    $("#orderDetailModal").modal('show');
  })

  $("#load-mode-btn").click(function (e) {
    e.preventDefault();
    const reviewBoxesCount = document.querySelectorAll(".review-comment").length;
    let url = window.location.href;
    url = url.replace("detail", "loadmore");
    url += `?skip=${reviewBoxesCount}`;
    fetch(url)
      .then(response => response.json())
      .then(data => {
        const total = data.reviewCount;
        const reviews = data.reviews;

        reviews.forEach(review => {
          const reviewBox = `
          <div class="review-comment mb--20">
            <div class="text">
              <div class="rating-block mb--15">
                ${generateStars(review.rate)}
              </div>
              <h6 class="author">
                ${review.fullName} – <span class="font-weight-400">${formatDate(review.createdAt)}</span>
              </h6>
              <p>
                ${review.text}
              </p>
            </div>
          </div>`;
          const reviewContainer = document.querySelector(".review-container");
          reviewContainer.innerHTML += reviewBox;
        });
        if (total <= reviewBoxesCount + reviews.length) {
          document.querySelector("#load-mode-btn").style.display = "none";
        }
      })
      .catch(error => {
        console.error('Error:', error);
      });
  });

  function generateStars(rate) {
    let starsHTML = '';
    for (let i = 1; i <= 5; i++) {
      starsHTML += `<span class="ion-android-star-outline ${i <= rate ? 'star_on' : ''}"></span>`;
    }
    return starsHTML;
  }

  function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  }


  $(".imgInput").change(function (e) {
    let box = $(this).parent().find(".preview-box");
    $(box).find(".previewImg").remove();

    for (var i = 0; i < e.target.files.length; i++) {

      let img = document.createElement("img");
      img.style.width = "200px";
      img.style.height = "200px";
      img.classList.add("previewImg");

      let reader = new FileReader();
      reader.readAsDataURL(e.target.files[i]);
      reader.onload = () => {
        img.setAttribute("src", reader.result);
        $(box).append(img)
      }
    }
  })

  $(".remove-img-icon").click(function () {
    $(this).parent().remove();
  })

  $(".bookImageField").mouseenter(function () {
    this.querySelector("#posterImageFieldId").style.display = "none";
    this.querySelector("#hoverImageFieldId").style.display = "inline-block";
  })

  $(".bookImageField").mouseleave(function () {
    this.querySelector("#posterImageFieldId").style.display = "inline-block";
    this.querySelector("#hoverImageFieldId").style.display = "none";
  })

  $(".delete-btn").click(function (e) {
    e.preventDefault();

    let url = $(this).attr("href");


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

        fetch(url)
          .then(response => {
            if (response.ok) {
              Swal.fire({
                title: "Deleted!",
                text: "Your file has been deleted.",
                icon: "success"
              }).then(() => {
                window.location.reload();
              })
            }
            else {
              Swal.fire({
                title: "Sorry!",
                text: "Something went wrong",
                icon: "error"
              })
            }
          })
      }
    });
  })
})