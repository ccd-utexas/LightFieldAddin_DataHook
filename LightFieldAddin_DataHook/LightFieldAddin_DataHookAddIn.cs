using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.AddIn;
using System.AddIn.Pipeline;
using System.Windows;

using PrincetonInstruments.LightField.AddIns;

namespace LightFieldAddIns
{
  ///////////////////////////////////////////////////////////////////////////
  //
  //  Purpose:
  //  - This addin hooks into the data stream when data is being acquired or 
  //    displayed and exports the buffer to LightField_View.fits.
  //  - This is a menu driven addin and sets up a check box menu 
  //    item as its source of control.
  //
  //  Notes:
  //  - Adapted from Online Sobel Sample:
  //    C:\Users\Public\Documents\Princeton Instruments\LightField
  //      \Add-in and Automation SDK\Samples\CSharp Add-Ins
  //  - It will only export the first region of interest.
  //  - It must be connected before clicking "Run Infinite" or "Acquire".
  //    Turning it on after the clicking "Run Infinite" or "Acquire" will do nothing.    
  //  - As recommended from http://heasarc.gsfc.nasa.gov/fitsio/fitsio.html,
  //    using CSharpFITS to export: http://vo.iucaa.ernet.in/~voi/CSharpFITS.html
  //  - For include additional functionality within this addin.
  //    Example: Make another class within this addin to save data as a fits
  //    file while acquiring.
  //  - Cannot use built-in IExportSettings methods to export to fits since they
  //    expect a SPE file object complete with XML footer metadata.
  //
  ///////////////////////////////////////////////////////////////////////////
  [AddIn("Data Hook",
	 Version = "0.0.1",
	 Publisher = "White Dwarf Research Group, Don Winget",
	 Description = "Hooks into data stream and overwrites temp file LightField_View.fits.")]
  public class AddinMenuDataHook : AddInBase, ILightFieldAddIn
  {
    bool? processEnabled_;
    bool menuEnabled_;
    DataHook dataHook_;
    IExperiment experiment_;
    
    ///////////////////////////////////////////////////////////////////////
    public UISupport UISupport { get { return UISupport.Menu; } }
    ///////////////////////////////////////////////////////////////////////
    public void Activate(ILightFieldApplication app)
    {
      // Capture Interface
      LightFieldApplication = app;
      experiment_ = app.Experiment;
      menuEnabled_ = CheckSystem();
      processEnabled_ = false;
      
      // Connect to experiment device changed:
      // When a camera is added, this add-in is active. 
      // When a camera is removed, this add-in is disabled.
      experiment_.ExperimentUpdated += experiment__ExperimentUpdated;
      
      // Connect to the data received event
      experiment_.ImageDataSetReceived += experimentDataReady;
      
      Initialize(Application.Current.Dispatcher, "Data Hook");
    }
    ///////////////////////////////////////////////////////////////////////
    void experiment__ExperimentUpdated(object sender, ExperimentUpdatedEventArgs e)
    {
      bool systemCheck = CheckSystem();
      
      // Update on change only
      if (menuEnabled_ != systemCheck)
	{
	  menuEnabled_ = systemCheck;
	  RequestUIRefresh(UISupport.Menu);
	}
      // Building a system can change sensor dimensions
      if (systemCheck)
	{
	  // Initialize online process and create data hook class
	  // if the system is ready
	  dataHook_ = new DataHook();
	}
    }
    ///////////////////////////////////////////////////////////////////////
    public void Deactivate()
    {
      // Stop listening to device changes
      experiment_.ExperimentUpdated -= experiment__ExperimentUpdated;
      
      // Disconnect Data Event            
      experiment_.ImageDataSetReceived -= experimentDataReady;
    }
    ///////////////////////////////////////////////////////////////////////
    public override string UIMenuTitle { get { return "Data Hook"; } }
    ///////////////////////////////////////////////////////////////////////
    public override bool UIMenuIsEnabled { get { return menuEnabled_; } }
    ///////////////////////////////////////////////////////////////////////
    public override bool? UIMenuIsChecked
    {
      get { return processEnabled_; }
      set { processEnabled_ = value; }
    }
    ///////////////////////////////////////////////////////////////////////        
    internal bool CheckSystem()
    {
      foreach (IDevice device in LightFieldApplication.Experiment.ExperimentDevices)
	{
	  if (device.Type == DeviceType.Camera)
	    return true;
	}
      // No Camera return false
      return false;
    }
    ///////////////////////////////////////////////////////////////////////        
    //  With all of the data in the buffer, export the first ROI in each frame to fits.
    ///////////////////////////////////////////////////////////////////////
    void experimentDataReady(object sender, ImageDataSetReceivedEventArgs e)
    {
      if (processEnabled_ == true) // NO-OP if its off on this event
	{
	  // Export the first ROI in the most recent frame to fits.
	  int regionIndex = 0;
	  long frameIndex = (long)(e.ImageDataSet.Frames - 1);
	  // Note: Capitalization error in documentation: GetFrameMetaData, not GetFrameMetadata
	  // Use Visual Studio "Go To Definition" to see.
	  dataHook_.ExportToFits(e.ImageDataSet.GetFrame(regionIndex, frameIndex),
				 e.ImageDataSet.GetFrameMetaData(frameIndex));
	}
    }
  }
  ///////////////////////////////////////////////////////////////////////
  //
  //  Hook into data stream.
  //
  ///////////////////////////////////////////////////////////////////////
  public class DataHook
  {
    ///////////////////////////////////////////////////////////////////////
    // Default constructor with no parameters.
    ///////////////////////////////////////////////////////////////////////
    public DataHook()
    {
    }
    ///////////////////////////////////////////////////////////////////////
    // Export to LightField_View.fits
    ///////////////////////////////////////////////////////////////////////
    public void ExportToFits(IImageData data, Metadata metadata)
    {
      // TODO: export to fits
      // obj to export: data.GetData()
      // TODO: insert call to csharpfits
      // from http://heasarc.gsfc.nasa.gov/fitsio/fitsio.html
      // from http://vo.iucaa.ernet.in/~voi/CSharpFITS.html
    }
  }
}
