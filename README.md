# Fractal Explorer for iOS

A simple Mandelbrot set explorer for iOS that takes advantage of the GPU using the Xamarin framework for iOS. The basic idea was to implement a calculation pipeline that continually refines the results from a previous calculation step and displays the intermediate results to give a visual feedback of how the calculation progresses. Because my iPad2 only supports OpenGL ES 2.0 I was restricted to using this older version instead of 3.0. Before the actual iterations are calculated a preview with a low resolution (8x8 pixels) is calculated and then used to render those pixels for which no precise results are available yet.

A good Mandelbrot set explorer gets pretty involved quickly, for example if you want to get the precision right for high zoom levels and reuse results when zooming and scrolling (like e.g. [Fractile Plus](https://itunes.apple.com/en/app/fractile-plus/id401591464?mt=8) does), in comparison, this application is very bare bones.

I am sure there is a lot that could be done to optimize this code, but the main goal was to experiment with a calculation pipeline that uses the GPU in order to get an idea how this could be applied to other problems.

## Screenshot 

![alt text](https://cloud.githubusercontent.com/assets/7856060/8895084/59fd8c84-33cd-11e5-9296-e6969c7c3550.png "Screenshot")

## Demo Video

See a demo video on [youtube](http://youtu.be/NVUj5CVOuwY).

## Credits
Some of the more general OpenGL functionality is based on this [code](https://github.com/xamarin/monotouch-samples/tree/master/OpenGL/OpenGLES20Example).
