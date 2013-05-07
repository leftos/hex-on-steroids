Hex on Steroids
	by Lefteris "Leftos" Aslanoglou

	
	Quick Notes
		This is my attempt at a special kind of hex editor. 
		- It can try to automatically detect the floats controlling NBA 2K shaders.
		- You can create profiles and set multiple byte-ranges; each range can even be interpreted differently than the others.


	Known Issues
		- Negative zeros are being handled and saved as positive zeros.


	Version History
		v0.5
			- Added byte jumps when auto detecting shaders or a custom header
			- When using Profiles > Edit..., the currently selected or last edited profile will be shown

		v0.4.1.1
			- Fixed Whole File not showing detected values for shaders with length less than the one set in the profile settings

		v0.4.1
			- Can now delete a range from a profile

		v0.4
			- Added count and auto end parameters to shaders
			- Added pasting to data ranges in Profiles window
			- Added option on whether to include headers when copying
			- Fixed various bugs

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


	License
		Copyright 2011-2013 Eleftherios Aslanoglou

		Licensed under the Apache License, Version 2.0 (the "License");
		you may not use this file except in compliance with the License.
		You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

		Unless required by applicable law or agreed to in writing, software
		distributed under the License is distributed on an "AS IS" BASIS,
		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
		See the License for the specific language governing permissions and
		limitations under the License.