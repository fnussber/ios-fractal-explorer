# Fractal Explorer for iOS

A simple Mandelbrot set explorer for iOS that takes advantage of the GPU. As it turns out this gets pretty involved if you want to get the precision right for high zoom levels and reuse results when zooming and scrolling (like e.g. [Fractile Plus](https://itunes.apple.com/en/app/fractile-plus/id401591464?mt=8) does), in comparison this application is pretty bare bones.

Nevertheless this was a good example to do some OpenGL GPU programming using the Xamarin framework for iOS. The basic idea was to implement a calculation pipeline that continually refines the results from a previous calculation step and displays the intermediate results to give a visual feedback of how the calculation progresses. Because my iPad2 only supports OpenGL ES 2.0 I was restricted to using this older version instead of 3.0.

## Screenshot 

![alt text](https://cloud.githubusercontent.com/assets/7856060/8895084/59fd8c84-33cd-11e5-9296-e6969c7c3550.png "Screenshot")

## Demo Video

See a demo video on [youtube](http://youtu.be/NVUj5CVOuwY).

## Credits
Some of the more general OpenGL functionality (like matrix manipulations) is based on this [code](https://github.com/xamarin/monotouch-samples/tree/master/OpenGL/OpenGLES20Example).
