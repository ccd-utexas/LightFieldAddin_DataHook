# LightFieldAddin_DataHook

This LightField Addin hooks into the data stream for use by other programs.

## Purpose:
- This addin hooks into the data stream when data is being acquired or 
  displayed and so that the data can be exported.

## Notes:
- The code commented within ExportToFits does not work. When this addin is enabled, LightField displays an error that the addin had to be deactivated and the experiment halts. In future, consider using File Sample addin from LightField to export a temporary SPE file.
- For further development, consider using IronPython instead of C#. The CSharpFITS library is not actively maintained. 
- This is a menu driven addin and sets up a check box menu item as its source of control.
- Adapted from Online Sobel Sample:
  C:\Users\Public\Documents\Princeton Instruments\LightField
    \Add-in and Automation SDK\Samples\CSharp Add-Ins
- It must be connected before clicking "Run Infinite" or "Acquire". Turning it on after the clicking "Run Infinite" or "Acquire" will do nothing.    
- As recommended from http://heasarc.gsfc.nasa.gov/fitsio/fitsio.html, using CSharpFITS to export: http://vo.iucaa.ernet.in/~voi/CSharpFITS.html
- Use this addin to create additional functionality.
  Example: Make another class within this addin to save data as a fits
  file while acquiring.
- Cannot use built-in IExportSettings methods to export to fits since they expect a SPE file object complete with XML footer metadata.

## How to contribute:
- On argos-dev2, open Microsoft  Visual Studio Professional 2012. (Visual Studio is required.)
- Open C:\Users\admin\Documents\GitHub\LightFieldAddin_DataHook\LightFieldAddin_DataHook.sln
- To compile: Build menu > click Clean. Then Build > Build Solution. The addin is now visible to LightField.
- Open LightField > Application Menu > Manage Addins... > Your Addins tab > select Data Hook. Then Application Menu > Addins > select Data Hook. Try acquiring data to test.

See
ftp://ftp.princetoninstruments.com/public/Manuals/Princeton%20Instruments/LightField%20Add-ins%20and%20Automation%20Programming%20Manual%20Ver%205.pdf
