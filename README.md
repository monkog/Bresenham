[![Build status](https://ci.appveyor.com/api/projects/status/on5axvlce94qvk73?svg=true)](https://ci.appveyor.com/project/monkog/simple-paint)
# :art: Simple paint
Simple paint allows you to compose figures from a set of lines and manipulate the created shapes. Every line is drawn pixel by pixel uisng Bresenham algorithm.

![Sample application usage](./.Docs/Program.gif)

## Bresenham's line algorithm
Bresenham's algorithm is a commonly used method for drawing lines. It's operating on integer values, which makes it quick, efficient and simple. Being so fast and cheep for computers to process, Bresenham's algorithm became widely used in many graphics libraries as well as in hardware (plotters, graphic cards). It's possible to extend the algorithm to draw circles as well.

The presented algorithm describes drawing lines for the first octant on the Cartesian Coordinate System.

In each step of the algorithm the `x` coordinate increment is constant `Δx=1`. The Bresenham's algorithm allows to compute the `y` coordinate increment. It can be either the E pixel or the NE pixel.

![Position of E and NE pixels](./.Docs/Bresenham.png)

1. Mark the line's start coordinates as (x<sub>0</sub>,y<sub>0</sub>) and (x<sub>1</sub>,y<sub>1</sub>)
2.  Compute the overall line's increment of x and y coordinates as `dx` and `dy`
 
	>![dx=x1-x0](https://latex.codecogs.com/gif.latex?dx&space;=&space;x_{1}-x_{0})  
	>![dy=y1-y0](https://latex.codecogs.com/gif.latex?dy&space;=&space;y_{1}-y_{0})  

3. Compute the precise step increment in `y` coordinate and the  E and NE approximations
 
	>![d=2dy-dx](https://latex.codecogs.com/gif.latex?d&space;=&space;2&space;dy-dx)  
	>![dE=2dy](https://latex.codecogs.com/gif.latex?dE&space;=&space;2&space;dy)  
	>![dNE=2(dy-dx)](https://latex.codecogs.com/gif.latex?dNE&space;=&space;2&space;(dy-dx))  

4. If `d < 0` draw the E pixel, otherwise draw the NE pixel

## :link: Useful links
:art: [Bresenham's algorithm on Wikipedia](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm) 

:art: [Bresenham's algorithm described for all octants in Cartesian Coordinate System](https://www.cs.helsinki.fi/group/goa/mallinnus/lines/bresenh.html)
