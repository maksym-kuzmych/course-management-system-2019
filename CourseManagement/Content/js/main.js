let slideIndex = 0;

const showSlides = () => {
  const slides = document.getElementsByClassName("mySlides");
  const dots = document.getElementsByClassName("dot");

  for (let i = 0; i < slides.length; i++) {
    slides[i].style.display = "none";
  }

  slideIndex++;
  if (slideIndex > slides.length) {
    slideIndex = 1;
  }

  for (let i = 0; i < dots.length; i++) {
    dots[i].className = dots[i].className.replace(" active", "");
    dots[i].id = `Dot${i + 1}`;
  }

  slides[slideIndex - 1].style.display = "block";
  dots[slideIndex - 1].className += " active";

  setTimeout(showSlides, 3000);

//   for (var el of dots) {
//     el.onclick = function(evt) {
//       if (evt.target.id === "Dot1") {
//         slideIndex = 0;
//         showSlides();
//       } else if (evt.target.id === "Dot2") {
//         slideIndex = 1;
//         showSlides();
//       } else if (evt.target.id === "Dot3") {
//         slideIndex = 2;
//         showSlides();
//       }
//     };
//   }
};

showSlides();
