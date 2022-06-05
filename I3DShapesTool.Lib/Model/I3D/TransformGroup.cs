using I3DShapesTool.Lib.Tools;
using System;
using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class TransformGroup
    {
        public string? Name { get; }
        public int? Id { get;}
        public Transform RelativeTransform { get; }
        public Transform AbsoluteTransform { get; private set; }
        public TransformGroup? Parent { get; private set; }
        public ISet<TransformGroup> Children { get; } = new HashSet<TransformGroup>();

        public TransformGroup(string? name, int? id, I3DVector translation, I3DVector rotation, I3DVector scale)
        {
            Name = name;
            Id = id;

            RelativeTransform = Transform.Identity
                .Scale(scale)
                .Rotate(rotation)
                .Translate(translation);
            AbsoluteTransform = RelativeTransform;
        }

        private void UpdateAbsoluteTransform()
        {
            if (Parent == null)
                throw new InvalidOperationException();

            AbsoluteTransform = Parent.AbsoluteTransform * RelativeTransform;
        }

        public void SetParent(TransformGroup parent)
        {
            if (Parent != null)
                Parent.Children.Remove(this);

            Parent = parent;
            Parent.Children.Add(this);

            UpdateAbsoluteTransform();

            // Refresh the absolute transform of all children since we have updated ours
            foreach (TransformGroup child in Children)
            {
                child.SetParent(this);
            }
        }

        public void PrintTree(int depth = 0)
        {
            var nameStr = Name != null ? $" {Name}" : "";
            var spaces = new string('-', depth);
            Console.WriteLine($"{spaces}<{GetType().Name}{nameStr}>");
            foreach (var child in Children)
            {
                child.PrintTree(depth + 1);
            }
        }
    }
}
