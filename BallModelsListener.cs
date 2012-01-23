using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletalTracking
{
    interface BallModelsListener
    {
        void handleBallAdded(BallModel ball);
        void handleBallRemoved(BallModel ball);
    }
}
