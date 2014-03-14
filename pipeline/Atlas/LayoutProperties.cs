using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameStack.Pipeline.Atlas
{
    public class LayoutProperties
    {
        public string[] inputFilePaths; 
        public int distanceBetweenImages; 
        public int marginWidth;
        public bool powerOfTwo;

        public LayoutProperties()
        {
            inputFilePaths = null;
            distanceBetweenImages = 0;
            marginWidth = 0;
            powerOfTwo = false;
        }
    }
}
