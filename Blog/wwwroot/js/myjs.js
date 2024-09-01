
  $('.slider-nav').slick({
	dots: true,
	arrows: true,
	infinite: true,
	speed: 300,
	slidesToShow: 5,
	slidesToScroll: 2,
	autoplay: true,
  	autoplaySpeed: 5000,
  	pauseOnHover: true,
	responsive: [
	  {
		breakpoint: 1024,
		settings: {
		  slidesToShow: 4,
		  slidesToScroll: 2,
		  infinite: true,
		  dots: true
		}
	  },
	  {
		breakpoint: 600,
		settings: {
		  slidesToShow: 2,
		  slidesToScroll: 2
		}
	  },
	  {
		breakpoint: 480,
		settings: {
		  slidesToShow: 2,
		  slidesToScroll: 2
		}
	  }
	  
	]
  });




  $('.slider-book').slick({
	dots: true,
	arrows: true,
	infinite: true,
	speed: 300,
	slidesToShow: 5,
	slidesToScroll: 3,
	autoplay: true,
  	autoplaySpeed: 5000,
  	pauseOnHover: true,
	responsive: [
	  {
		breakpoint: 1024,
		settings: {
		  slidesToShow: 4,
		  slidesToScroll: 2,
		  infinite: true,
		  dots: true
		}
	  },
	  {
		breakpoint: 600,
		settings: {
		  slidesToShow: 2,
		  slidesToScroll: 2
		}
	  },
	  {
		breakpoint: 480,
		settings: {
		  slidesToShow: 2,
		  slidesToScroll: 2
		}
	  }
	  
	]
  });


  $('.slider-resource').slick({
	dots: true,
	arrows: true,
	infinite: true,
	speed: 300,
	slidesToShow: 4,
	slidesToScroll: 2,
	// autoplay: true,
  	// autoplaySpeed: 5000,
  	pauseOnHover: true,
	responsive: [
	  {
		breakpoint: 1024,
		settings: {
		  slidesToShow: 4,
		  slidesToScroll: 2,
		  infinite: true,
		  dots: true
		}
	  },
	  {
		breakpoint: 600,
		settings: {
		  slidesToShow: 2,
		  slidesToScroll: 2
		}
	  },
	  {
		breakpoint: 480,
		settings: {
		  slidesToShow: 1,
		  slidesToScroll: 1
		}
	  }
	  
	]
  });



  