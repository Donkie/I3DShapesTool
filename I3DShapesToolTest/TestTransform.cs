using I3DShapesTool.Lib.Model;
using I3DShapesTool.Lib.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace I3DShapesToolTest
{
    public class TestTransform
    {
        [Fact]
        public void TestTranslate()
        {
            Transform t = Transform.Identity
                .Translate(new I3DVector(1, -1, 1));

            I3DVector v = t * new I3DVector(10, 11, 12);
            Assert.Equal(11, v.X, 6);
            Assert.Equal(10, v.Y, 6);
            Assert.Equal(13, v.Z, 6);
        }

        [Fact]
        public void TestRotateX()
        {
            Transform t = Transform.Identity
                .Rotate(new I3DVector(Math.PI / 2, 0, 0));

            I3DVector v = t * new I3DVector(0, 1, 0);
            Assert.Equal(0, v.X, 6);
            Assert.Equal(0, v.Y, 6);
            Assert.Equal(1, v.Z, 6);
        }

        [Fact]
        public void TestRotateY()
        {
            Transform t = Transform.Identity
                .Rotate(new I3DVector(0, Math.PI / 2, 0));

            I3DVector v = t * new I3DVector(1, 0, 0);
            Assert.Equal(0, v.X, 6);
            Assert.Equal(0, v.Y, 6);
            Assert.Equal(-1, v.Z, 6);
        }

        [Fact]
        public void TestRotateZ()
        {
            Transform t = Transform.Identity
                .Rotate(new I3DVector(0, 0, Math.PI / 2));

            I3DVector v = t * new I3DVector(1, 0, 0);
            Assert.Equal(0, v.X, 6);
            Assert.Equal(1, v.Y, 6);
            Assert.Equal(0, v.Z, 6);
        }

        [Fact]
        public void TestRotateXY()
        {
            Transform t = Transform.Identity
                .Rotate(new I3DVector(Math.PI / 2, Math.PI / 2, 0));

            I3DVector v = t * new I3DVector(1, 0, 0);
            Assert.Equal(0, v.X, 6);
            Assert.Equal(1, v.Y, 6);
            Assert.Equal(0, v.Z, 6);
        }

        [Fact]
        public void TestScale()
        {
            Transform t = Transform.Identity
                .Scale(new I3DVector(0.5, 0.5, 0.5));

            I3DVector v = t * new I3DVector(1, 0, 0);
            Assert.Equal(0.5, v.X, 6);
            Assert.Equal(0, v.Y, 6);
            Assert.Equal(0, v.Z, 6);
        }

        [Fact]
        public void TestScaleTwice()
        {
            Transform t = Transform.Identity
                .Scale(new I3DVector(0.5, 0.5, 0.5))
                .Scale(new I3DVector(2, 2, 2));

            I3DVector v = t * new I3DVector(1, 0.3, -0.7);
            Assert.Equal(1, v.X, 6);
            Assert.Equal(0.3, v.Y, 6);
            Assert.Equal(-0.7, v.Z, 6);
        }

        [Fact]
        public void TestScaleTranslateRotate()
        {
            Transform t = Transform.Identity
                .Scale(new I3DVector(0.5, 0.5, 0.5))
                .Translate(new I3DVector(6, 0, 0))
                .Rotate(new I3DVector(0, Math.PI / 2, 0));

            I3DVector v = t * new I3DVector(2, 0, 0);
            Assert.Equal(0, v.X, 6);
            Assert.Equal(0, v.Y, 6);
            Assert.Equal(-7, v.Z, 6);
        }
    }
}
