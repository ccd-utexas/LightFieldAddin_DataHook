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
    // NOTE: Adapted from Sobel transform example
    //
    //  This sample hooks into the data stream when data is being acquired or 
    // displayed and modifies the data by performing a sobel edge detection on 
    // the buffer(s). This is a menu driven addin and sets up a check box menu 
    // item as its source of control.
    //
    //  Notes: It will only sobel transform the first region of interest.
    //         It must be connected before acquiring or focusing, turning it
    //         on after the acquisition is started will do nothing.    
    //
    //  This sample shows how to inject your code into the LightField data 
    //  stream and modify the data.
    //
    ///////////////////////////////////////////////////////////////////////////
    [AddIn("Data Hook",
            Version = "0.0.1",
            Publisher = "White Dwarf Research Group, Don Winget",
            Description = "Hooks into data stream and outputs temp file LightField_View.fits.")]
    public class AddinMenuDataHook : AddInBase, ILightFieldAddIn
    {
        bool? processEnabled_;
        bool menuEnabled_;
        // TODO: replace with reference to class for outputing data
        // HookToFits hookToFits_;
        // RemotingSobelTransformation sobelTransformer_;
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

            // Listen to region of interest result changed and re-compute the buffers to match the 
            // region
            List<string> settings = new List<string>();
            settings.Add(CameraSettings.ReadoutControlRegionsOfInterestResult);
            experiment_.FilterSettingChanged(settings);
            experiment_.SettingChanged += experiment__SettingChanged;

            // Connect to experiment device changed (when camera is added this add-in is active, and 
            // if a camera is removed then this add-in is disabled.
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
            // Building a system can change the sensor dimensions 
            if (systemCheck)
            {
                // Initialize Online Process and create transformation class
                // If the system is ready
                RegionOfInterest[] rois = experiment_.SelectedRegions;
                // TODO: replace with reference to class for outputting data
                // hookToFits_ = new HookToFits(rois);
                // sobelTransformer_ = new RemotingSobelTransformation(rois);
            }
        }
        ///////////////////////////////////////////////////////////////////////
        void experiment__SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (CheckSystem())
            {
                // Initialize Online Process and create transformation class
                RegionOfInterest[] rois = experiment_.SelectedRegions;
                // TODO: replace with call to output data
                // hookToFits_ = new HookToFits(rois);
                // sobelTransformer_ = new RemotingSobelTransformation(rois);
            }
        }
        ///////////////////////////////////////////////////////////////////////
        public void Deactivate()
        {
            // Stop listening to device changes
            experiment_.ExperimentUpdated -= experiment__ExperimentUpdated;

            // Stop snooping settings
            experiment_.FilterSettingChanged(new List<string>());
            experiment_.SettingChanged -= experiment__SettingChanged;

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
        //  With all of the data in the block, transform it all        
        ///////////////////////////////////////////////////////////////////////
        void experimentDataReady(object sender, ImageDataSetReceivedEventArgs e)
        {
            if (processEnabled_ == true) // NO-OP if its off on this event
            {
                // Are we transforming the data? Transform all frames in the package
                // TODO: only take first roi?
                for (int i = 0; i < (int)e.ImageDataSet.Frames; i++)
                    for (int roi = 0; roi < e.ImageDataSet.Regions.Length; roi++)
                        // TODO: insert call to csharpfits
                        // from http://heasarc.gsfc.nasa.gov/fitsio/fitsio.html
                        // from http://vo.iucaa.ernet.in/~voi/CSharpFITS.html
                        // TODO:    replace with reference to instance of data hook class
                        // hookToFits_.BinToFits(e.ImageDataSet.GetFrame(roi, i), roi);
                        // sobelTransformer_.Transform(e.ImageDataSet.GetFrame(roi, i), roi);
                        continue;
            }
        }
    }
    ///////////////////////////////////////////////////////////////////////
    // NOTE: This class is commented out where referenced.
    // TODO: Replace all instances of RemoteSobelTransformation with
    //       class to output data (via ironpython?)
    //
    //  Perform a Sobel Transformation on all regions
    //
    ///////////////////////////////////////////////////////////////////////
    public class HookToFits
    {
        static int[][, ,] indexBuffers_;
        static ushort[][] retData;

    ///////////////////////////////////////////////////////////////////////
    // NOTE: This class is commented out where referenced.
    // TODO: Replace all instances of RemoteSobelTransformation with
    //       class to output data (via ironpython?)
    //
    //  Perform a Sobel Transformation on all regions
    //
    ///////////////////////////////////////////////////////////////////////
    public class RemotingSobelTransformation
    {
        static int[][, ,] indexBuffers_;
        static ushort[][] retData_;

        // Matrices
        ///////////////////////////////////////////////////////////////////////
        double[] gy = new double[] { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
        double[] gx = new double[] { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        int[] xs = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        int[] ys = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        ///////////////////////////////////////////////////////////////////////
        // Build Buffers of Indexes On Construction For Speed
        ///////////////////////////////////////////////////////////////////////
        public RemotingSobelTransformation(RegionOfInterest[] rois)
        {
            // Allocate outer buffers
            indexBuffers_ = new int[rois.Length][, ,];
            retData_ = new ushort[rois.Length][];

            for (int roiIndex = 0; roiIndex < rois.Length; roiIndex++)
            {
                int dW = rois[roiIndex].Width / rois[roiIndex].XBinning;
                int dH = rois[roiIndex].Height / rois[roiIndex].YBinning;

                // Static Computed Once Upon Starting or roi changing
                // Compute all of the indexes ahead of time, this makes the 
                // code to do the process 10 times faster or more.
                if ((indexBuffers_[roiIndex] == null) || (indexBuffers_[roiIndex].Length != dW * dH * 9))
                {
                    indexBuffers_[roiIndex] = new int[dW, dH, 9];
                    for (int xx = 2; xx < dW - 2; xx++)
                    {
                        for (int yy = 2; yy < dH - 2; yy++)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                int index = (xx + xs[i]) + (yy + ys[i]) * dW;
                                indexBuffers_[roiIndex][xx, yy, i] = index;
                            }
                        }
                    }
                }
                // Output Data Buffers
                retData_[roiIndex] = new ushort[dW * dH];
            }
        }
        ///////////////////////////////////////////////////////////////////////
        // Perform the actual transformation
        ///////////////////////////////////////////////////////////////////////
        public void Transform(IImageData data, int roi)
        {
            // Hint use locals (accessing on Interface forces boundary crossing)
            // that will be unbearably slow.
            int dW = data.Width;    // Boundary Crossing
            int dH = data.Height;   // Boundary Crossing

            ushort[] ptr = (ushort[])data.GetData();     // Input Data

            // Loop Width & Height(Quick and Dirty Padding Of 2) 
            // This Avoids a lot of boundary checking or reflection and increases speed
            for (int xx = 2; xx < dW - 2; xx++)
            {
                for (int yy = 2; yy < dH - 2; yy++)
                {
                    double GY = 0, GX = 0;
                    // Compute the X and Y Components
                    for (int i = 0; i < 9; i++)
                    {
                        int idx = indexBuffers_[roi][xx, yy, i];
                        GY += ptr[idx] * gy[i];
                        GX += ptr[idx] * gx[i];
                    }
                    // Magnitude
                    double G = Math.Sqrt(GX * GX + GY * GY);

                    // Put the Magnitude into the output buffer
                    retData_[roi][yy * dW + xx] = (ushort)G;
                }
            }
            // Write the output buffer to the IImageData
            // Boundary Crossing
            data.SetData(retData_[roi]);
        }
    }
}

