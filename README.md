This repository containing differen one file raylib examples.
It uses the F# Scripting ability (dotnet fsi).

The purpose is either to just show basic Raylib usage, or exploring
different concepts related to game development.

# circle.fsx

Uses Polar coordinate and conversation to cartesian to create circles

# euler.fsx

A physics simulation using Euler Integration. Not so good at the moment.
Better look at verlet.fsx.

# inside_poly.fsx

Algorithm to check if a certain point is inside a concave mesh

# mouse_follow.fsx

A circle following the mouse cursor, drawing on a render texture while
doing so.

# moveable.fsx

Drag 'n Drop example

# particle_system.fsx

A particle system

# polar.fsx

Another example using Polar coordinates to create some "art".

# raycast.fsx

Algorithm that checks if a certain ray intersects with a line.

# render_texture.fsx

Drawing to a permanent rendering texture.

# snowflake.fsx

Shows an animation of a koch fractal snowflake. Repeats forever.

# snowflake2.fsx

Let's you draw lines and then split every line with the koch fractal "algorithm".

# spatial.fsx

Visualization for a Spatial Tree implementation. You can drag'n'drop points and add new
points with right mouse click. Shows the initialized cells in the Spatial Tree and when
you move inside a cell which points are inside.

A spatial tree is for example used in efficent physic collision systems.

# verlet.fsx

Uses Verlet integration for physics simulation. Another example of doing physics compared
to `euler.fsx` that used Euler Integration. Also uses the Spatial Tree.

# verlet_sticks.fsx

Mass aggegragte physic system that uses verlet integration. Multiple points are connected to
form a restriction. This way different shapes like boxes, ropes or soft-body physics can be
created and simulated.
