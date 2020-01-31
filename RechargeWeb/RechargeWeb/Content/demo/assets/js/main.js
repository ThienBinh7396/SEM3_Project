(function ($) {
	"use strict";
    jQuery(document).ready(function($){

        var $slickNav = $('#main-menu');
        $slickNav.slicknav({
            prependTo: '.responsive-menu',
            label: ''
        });
        //ripple show	
        $('.header-area-ripple').ripples({
            dropRadius: 20,
            perturbance: 0.02,
        });
        //back to top 
        $(document).on('click', '.back-to-top', function () {
                $("html,body").animate({
                    scrollTop: 0
                }, 2000);
        });
        //smoth achor effect
        $('#main-menu li a').on('click', function (e) {
            e.preventDefault();
            var anchor = $(this).attr('href');
            var top = $(anchor).offset().top;
            $('html, body').animate({
                scrollTop: $(anchor).offset().top
            }, 1000);
            $(this).parent().addClass('active').siblings().removeClass('active');
        });
        //header-slider
        var $headerSlider = $('.header-slider-carousel');
        $headerSlider.owlCarousel({
            loop: true,
            dots: false,
            nav: true,
            autoplay:true,
            autoPlayTimeout: 1000,
            navText: ['<i class="fas fa-angle-left"></i>', '<i class="fas fa-angle-right"></i>'],
            responsive: {
                0: {
                    items: 1
                },
                768: {
                    items: 1
                },
                960: {
                    items: 1
                },
                1200: {
                    items: 1
                },
                1920: {
                    items: 1
                }
            }
        });
       $headerSlider.on("translate.owl.carousel", function () {
            $(".single-header-slide-item h1, .single-header-slide-item p").removeClass(" fadeInUp").css("opacity", "0");
           $(".header-btn-group a").removeClass("animated fadeInDown").css("opacity", "0");
        });

        $headerSlider.on("translated.owl.carousel", function () {
            $(".single-header-slide-item h1, .single-header-slide-item p").addClass("animated fadeInUp").css("opacity", "1");
            $(".header-btn-group a").addClass("animated fadeInDown").css("opacity", "1");
        });
        
        //circle magic
        if ($('.header-circle-magic').length == 1) {
            $('.header-circle-magic').circleMagic({
                elem: '.header-circle-magic',
                color: 'rgba(56, 128, 244,.5)',
                radius: 10,
                densety: 0.5,
                clearOffset: 0.2
            });
        }
    });
    $(window).on('scroll', function () {
        //back to top show/hide
       var ScrollTop = $('.back-to-top');
       if ($(window).scrollTop() > 1000) {
           ScrollTop.fadeIn(1000);
       } else {
           ScrollTop.fadeOut(1000);
       }
       /*--sticky menu activation--*/
       var mainMenuTop = $('.navbar-area');
       if ($(window).scrollTop() > 1000) {
           mainMenuTop.addClass('nav-fixed');
       } else {
           mainMenuTop.removeClass('nav-fixed');
       }
       
    });
           
    $(window).on('load',function(){
        //preloader
        var preLoder = $(".preloader");
        preLoder.fadeOut(500);
    });

}(jQuery));	
