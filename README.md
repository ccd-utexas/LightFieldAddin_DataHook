LightFieldAddin_DataHook
========================

This LightField Addin hooks into the data stream for use by other programs.

This addin hooks into the data stream when data is being acquired or 
displayed and exports the buffer to LightField_View.fits.
This is a menu driven addin and sets up a check box menu 
item as its source of control.

 Notes:
 - Adapted from Online Sobel Sample:
 C:\Users\Public\Documents\Princeton Instruments\LightField\Add-in and
 Automation SDK\Samples\CSharp Add-Ins
- The addin will only read the first region of interest.
- It must be connected before acquiring or focusing, turning it on after the acquisition is started will do nothing.

For include additional functionality within this addin.
 Example: Make another class within this addin to save data as a fits
 file while acquiring.

See
ftp://ftp.princetoninstruments.com/public/Manuals/Princeton%20Instruments/LightField%20Add-ins%20and%20Automation%20Programming%20Manual%20Ver%205.pdf
