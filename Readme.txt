Hex on Steroids
	by Lefteris "Leftos" Aslanoglou

	
	Quick Notes
		This is my attempt at a special kind of hex editor. 
		- It can try to automatically detect the floats controlling NBA 2K shaders.
		- You can create profiles and set multiple byte-ranges; each range can even be interpreted differently than the others.


	Known Issues
		- Negative zeros are being handled and saved as positive zeros.


	Version History
		v0.3.0.3
			- Fixed pasting to the last row

		v0.3.0.2
			- Changed (fixed?) point at which Auto Detect Shaders starts reading data

		v0.3.0.1
			- Fixes in Auto Detect with Custom Header feature

		v0.3
			- Added Auto Detect with Custom Header feature

		v0.2.0.1
			- Fixed saving behavior when using value constraints

		v0.2
			- Added status bar which indicates current decimal offset and original value
			- Fixed bug which wouldn't load values of whole files correctly if the data type wasn't Double
			- Tool now shows progress in the title bar during most lengthy operations
			- Profiles now have bounds option, which allows you to load only values between certain bounds
			- Profiles now have absolute value option, which allows you to load only values larger in absolute value than a specified one
			- Improved shader detection by making sure not to start parsing at an odd offset
			- Tool now supports pasting values back into it
			- Go to Offset added

		v0.1
			- Initial Release