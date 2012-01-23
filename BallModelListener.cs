using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletalTracking
{
    interface BallModelListener
    {
        void handleBallModelChanged(BallModel ball);
    }
}
