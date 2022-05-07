using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public struct BrainMovement
    {
        public bool Jump { get; set; } 
        public bool Left { get; set; } 
        public bool Right { get; set; }

        public BrainMovement(bool jump = false, bool left = false, bool right = false)
        {
            Jump = jump;
            Left = left;
            Right = right;
        }

        public BrainMovement Clone()
        {
            return new BrainMovement(this.Jump, this.Left, this.Right);
        }
    }
}
