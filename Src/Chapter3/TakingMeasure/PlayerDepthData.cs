/*
 * 
 *  Copyright (c) 2012 Jarrett Webb & James Ashley
 * 
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 *  documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 *  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 *  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 *  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 *  IN THE SOFTWARE.
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeginningKinect.Chapter3.TakingMeasure
{
    public class PlayerDepthData
    {
        #region Member Variables
        private const double MillimetersPerInch         = 0.0393700787;
        private static readonly double HorizontalTanA   = Math.Tan(57.0 / 2.0 * Math.PI / 180);
        private static readonly double VerticalTanA     = Math.Abs(Math.Tan(43.0 / 2.0 * Math.PI / 180));

        private int _DepthSum;
        private int _DepthCount;
        private int _LoWidth;
        private int _HiWidth;
        private int _LoHeight;
        private int _HiHeight;
        #endregion Member Variables


        #region Constructor
        public PlayerDepthData(int playerId, double frameWidth, double frameHeight)
        {
            this.PlayerId       = playerId;
            this.FrameWidth     = frameWidth;
            this.FrameHeight    = frameHeight; 


            this._LoWidth      = int.MaxValue;
            this._HiWidth      = int.MinValue;
            
            this._LoHeight      = int.MaxValue;
            this._HiHeight      = int.MinValue;               
        }
        #endregion Constructor


        #region Methods
        public void UpdateData(int x, int y, int depth)
        {
            this._DepthCount++;
            this._DepthSum     += depth;
            this._LoWidth       = Math.Min(this._LoWidth, x);
            this._HiWidth       = Math.Max(this._HiWidth, x);
            this._LoHeight      = Math.Min(this._LoHeight, y);
            this._HiHeight      = Math.Max(this._HiHeight, y);
        }
        #endregion Methods


        #region Properties
        public int PlayerId { get; private set; } 
        public double FrameWidth { get; private set; }
        public double FrameHeight { get; private set; }


        public double Depth
        {
            get { return this._DepthSum / (double) this._DepthCount; }
        }


        public int PixelWidth
        {
            get { return this._HiWidth - this._LoWidth; }
        }


        public int PixelHeight
        {
            get { return this._HiHeight - this._LoHeight; }
        }
        
        
        public string RealWidth
        {
            get 
            { 
                double inches = this.RealWidthInches;
                int feet = (int) (inches / 12);
                inches %= 12;
                
                return string.Format("{0}'{1:0.0}\"", feet, inches);
            }
        }        


        public string RealHeight
        {
            get 
            { 
                double inches = this.RealHeightInches;
                int feet = (int) (inches / 12);
                inches %= 12;
                
                return string.Format("{0}'{1:0.0}\"", feet, inches);
            }
        }


        public double RealWidthInches 
        { 
            get
            {
                double opposite = this.Depth * HorizontalTanA;
                return this.PixelWidth * 2 * opposite / this.FrameWidth * MillimetersPerInch;
            }
        }

        public double RealHeightInches 
        { 
            get
            {
                double opposite = this.Depth * VerticalTanA;
                return this.PixelHeight * 2 * opposite / this.FrameHeight * MillimetersPerInch;
            }
        }
        #endregion Properties
    }
}
