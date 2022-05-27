# Lua Modules

## camera
Module for controlling the world camera

### Methods:
	 set_zoom(zoom) 

Sets the camera's zoom level [0-1]

	 enable_control() 

Enables the camera's input control

	 disable_control() 

Disables the camera's input control

	 animate(coords speed loop) 

Moves the camera from one coordinate to the next in a list of coordinates

	 on_animation_finished(evt) 

Called when a camera animation finished

### Properties:
	 coord 

Gets/Sets the camera's position in coordinates

## geo
Module for geo operations

### Methods:
	 distance(coord1 coord2) 

Calculates the distance in kilometer between two coordinates

## io
Module for loading image and text

### Methods:
	 load(file_path) 

Loads a text(txt, json, geojson) or image file(jpg, png) from the data path

	 load_image(file_path format) 

Loads an image file (jpg, png) from the data path. You can optionally specify a format for the image. Currently supported formats are: R8, RG8, and RGBA8 (default)

### Properties:
	 data_dir 

Returns the directory path that data can be loaded from (read only)

## random
Module for generating random numbers and more

### Methods:
	 set_seed(seed) 

Sets the seed of the random generator

	 number() 

returns a random number between 0 [inclusive] and 1 (exclusive)

	 number(min max) 

returns a random number between min [inclusive] and max (exclusive)

	 whole_number(min max) 

returns a random whole number between min [inclusive] and max (exclusive)

	 coord() 

returns a random coordinate

	 color(saturation brightness) 

returns a color with a random hue

## controls
Module for creating a range of different controls

### Methods:
	 rebuild_controls(dataset) 

Rebuilds the control ui for the specified dataset

	 number(name default_value) 

A control holding a number

	 range(name default_value min max) 

A control holding a number within a range

	 interval(name default_lower default_upper min max) 

A control holding an interval with a lower and upper bound

	 vector2(name default_value) 

A control holding a two dimensional vector

	 toggle(name default_value) 

A control holding a boolean value

	 text(name default_value) 

A control holding text

	 choice(name default_value choices choice_display_names) 

A control holding a set of choices

	 vector(name default_value) 

A control holding a vector

	 color(name default_value) 

A control holding a color

## datasets
Module to access the world mod datasets

### Methods:
	 add(name) 

Adds a dataset to the list of available datasets

	 all() 

Returns a collection of all datasets

	 get(name) 

Gets the dataset with the specified name

## features
Module to control the world features

### Methods:
	 create(name) 

Creates a feature collection

	 add(features) 

Adds a feature collection to the world

	 remove(features) 

Adds a feature collection to the world

## localization
Module to access localization functions

### Methods:
	 get_locales() 

Returns a list of all available locales

	 avtivate_locale(locale) 

Activates the specified locale (e.g. en-US).

	 add_locale(name language territory) 

Adds a locale to the localization system

	 string(key local_strings) 

Creates a localized string

	 get(key) 

Returns the localized string for the current locale

	 import(csv) 

Import localization data from a csv file

### Properties:
	 current_locale 

Returns the currently active locale (Read only)

## log
Module to print messages to the log

### Methods:
	 print(message) 

Prints a messages to the log

	 clear() 

Clears the log

## projection
Module to manipulate the projection

### Methods:
	 scale(scale) 

Changes the scale of the projection

	 offset_x(x) 

Changes the x offset of the projection

	 offset_y(y) 

Changes the x offset of the projection

## sequence
Module to alter the data layers

### Methods:
	 add(dataset) 

Adds a dataset to the end of the sequence

	 insert(dataset index) 

Inserts a dataset into the sequence

	 remove(dataset) 

Removes a dataset from the sequence

	 on_change(callback) 

Adds a function to be executed when the sequence changes

	 index_of(proxy) 

Returns the index of the dataset in the sequence. Returns 0 if the dataset is not in the sequence

## sun
Module to alter the sun position

### Methods:
	 follow_view() 

Binds the suns position to the current view

### Properties:
	 zenith 

Returns the coordinate that is currently in zenith

## synth
Module to alter the data layers

### Methods:
	 create(channel) 

Creates a synth layer for a specified channel

	 update(channel) 

Tells the synthesizer to update the specified channel until the next frame

## time
Module for accessing time data

### Properties:
	 time 

The time at the beginning of this frame

	 delta 

The interval in seconds from the last frame to the current frame

	 idle_time 

The time since the last interaction with the application

## tutorial
Module to set the tutorial overlay


