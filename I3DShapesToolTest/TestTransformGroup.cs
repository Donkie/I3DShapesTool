using I3DShapesTool.Lib.Model;
using I3DShapesTool.Lib.Model.I3D;
using Xunit;
using System.Linq;

namespace I3DShapesToolTest
{
    public class TestTransformGroup
    {
        [Fact]
        public void TestParenthood()
        {
            TransformGroup tg1 = new TransformGroup(null, null, I3DVector.Zero, I3DVector.Zero, I3DVector.One);
            TransformGroup tg2 = new TransformGroup(null, null, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

            Assert.Null(tg1.Parent);
            Assert.Null(tg2.Parent);

            tg2.SetParent(tg1);

            Assert.Same(tg1, tg2.Parent);
            Assert.True(tg1.Children.Count == 1);
            Assert.True(tg1.Children.First() == tg2);
        }

        [Fact]
        public void TestTransforms()
        {
            TransformGroup tg1 = new TransformGroup(null, null, I3DVector.Zero, I3DVector.Zero, new I3DVector(0.5, 0.5, 0.5));
            TransformGroup tg2 = new TransformGroup(null, null, new I3DVector(6, 0, 0), I3DVector.Zero, I3DVector.One);
            TransformGroup tg3 = new TransformGroup(null, null, I3DVector.Zero, new I3DVector(0, 90, 0), I3DVector.One);

            tg3.SetParent(tg2);
            tg2.SetParent(tg1);

            I3DVector v = tg3.AbsoluteTransform * new I3DVector(2, 0, 0);
            Assert.Equal(3, v.X, 6);
            Assert.Equal(0, v.Y, 6);
            Assert.Equal(-1, v.Z, 6);
        }
    }
}
