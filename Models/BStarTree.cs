using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using L2.Models;
using System.IO;

namespace L2.Models
{
    public class BStarTree<T> : IEnumerable<T> where T : IComparable
    {
        public BTreeNode<T> Root;
        private readonly int grade;
        private readonly int maxKeys;
        private readonly int minKeys;
        private readonly int maxKeysRoot;

        public BStarTree(int grade)
        {
            this.grade = grade;
            maxKeys = grade - 1;
            minKeys = ((2 * grade) - 1) / 3;
            maxKeysRoot = (4 * (grade - 1)) / 3;
        }

        public bool Search(T value)
        {
            return Seeker(Root, value) != null;
        }

        public BTreeNode<T> Seeker(BTreeNode<T> node, T value)
        {
            if (node.IsLeaf)
            {
                for (var i = 0; i < node.KeyCount; i++)
                {
                    if (value.CompareTo(node.Keys[i]) == 0)
                    {
                        return node;
                    }
                }
            }
            else
            {
                for (var i = 0; i < node.KeyCount; i++)
                {
                    if (value.CompareTo(node.Keys[i]) == 0)
                    {
                        return node;
                    }

                    if (value.CompareTo(node.Keys[i]) < 0)
                    {
                        return Seeker(node.Children[i], value);
                    }

                    if (node.KeyCount == i + 1)
                    {
                        return Seeker(node.Children[i + 1], value);
                    }
                }

            }

            return null;
        }

        public void Insert(T value)
        {
            if (Root == null)
            {
                Root = new BTreeNode<T>(maxKeysRoot, null) { Keys = { [0] = value } };
                Root.KeyCount++;
            }
            else
            {
                var leafToInsert = SeekerInsertionLeaf(Root, value);
                InsertSplit(ref leafToInsert, value, null, null);
            }
        }

        private BTreeNode<T> SeekerInsertionLeaf(BTreeNode<T> node, T value)
        {
            if (node.IsLeaf)
            {
                return node;
            }

            for (var i = 0; i < node.KeyCount; i++)
            {
                if (value.CompareTo(node.Keys[i]) < 0)
                {
                    return SeekerInsertionLeaf(node.Children[i], value);
                }

                if (node.KeyCount == i + 1)
                {
                    return SeekerInsertionLeaf(node.Children[i + 1], value);
                }
            }

            return node;
        }

        private void InsertSplit(ref BTreeNode<T> node, T value, BTreeNode<T> leftValue, BTreeNode<T> rightValue)
        {
            if (node == null)
            {
                node = new BTreeNode<T>(maxKeysRoot, null);
                Root = node;
            }

            if (node.KeyCount != maxKeys)
            {
                InsertToNotFullNode(ref node, value, leftValue, rightValue);
                return;
            }

            var left = new BTreeNode<T>(maxKeys, null);
            var right = new BTreeNode<T>(maxKeys, null);
            var currentMedianIndex = node.GetHalfIndex();
            var currentNode = left;
            var currentNodeIndex = 0;
            var newHalf = default(T);
            var newHalfSet = false;
            var valueInserted = false;
            var insertionCount = 0;

            for (var i = 0; i < node.KeyCount; i++)
            {
                if (!newHalfSet && insertionCount == currentMedianIndex)
                {
                    newHalfSet = true;

                    if (!valueInserted && value.CompareTo(node.Keys[i]) < 0)
                    {
                        newHalf = value;
                        valueInserted = true;

                        if (leftValue != null)
                        {
                            SetChild(currentNode, currentNode.KeyCount, leftValue);
                        }

                        currentNode = right;
                        currentNodeIndex = 0;

                        if (rightValue != null)
                        {
                            SetChild(currentNode, 0, rightValue);
                        }

                        i--;
                        insertionCount++;
                        continue;
                    }

                    newHalf = node.Keys[i];
                    currentNode = right;
                    currentNodeIndex = 0;
                    continue;
                }

                if (valueInserted || node.Keys[i].CompareTo(value) < 0)
                {
                    currentNode.Keys[currentNodeIndex] = node.Keys[i];
                    currentNode.KeyCount++;

                    if (currentNode.Children[currentNodeIndex] == null)
                    {
                        SetChild(currentNode, currentNodeIndex, node.Children[i]);
                    }

                    SetChild(currentNode, currentNodeIndex + 1, node.Children[i + 1]);
                }
                else
                {
                    currentNode.Keys[currentNodeIndex] = value;
                    currentNode.KeyCount++;
                    SetChild(currentNode, currentNodeIndex, leftValue);
                    SetChild(currentNode, currentNodeIndex + 1, rightValue);
                    i--;
                    valueInserted = true;
                }

                currentNodeIndex++;
                insertionCount++;
            }

            if (!valueInserted)
            {
                currentNode.Keys[currentNodeIndex] = value;
                currentNode.KeyCount++;
                SetChild(currentNode, currentNodeIndex, leftValue);
                SetChild(currentNode, currentNodeIndex + 1, rightValue);
            }

            var parent = node.Parent;
            InsertSplit(ref parent, newHalf, left, right);
        }

        private void InsertToNotFullNode(ref BTreeNode<T> node, T value, BTreeNode<T> leftValue, BTreeNode<T> rightValue)
        {
            var inserted = false;

            for (var i = 0; i < node.KeyCount; i++)
            {
                if (value.CompareTo(node.Keys[i]) >= 0)
                {
                    continue;
                }

                InsertAt(node.Keys, i, value);
                node.KeyCount++;
                SetChild(node, i, leftValue);
                InsertChild(node, i + 1, rightValue);
                inserted = true;
                break;
            }

            if (inserted)
            {
                return;
            }

            node.Keys[node.KeyCount] = value;
            node.KeyCount++;
            SetChild(node, node.KeyCount - 1, leftValue);
            SetChild(node, node.KeyCount, rightValue);
        }

        private void Balance(BTreeNode<T> node)
        {
            if (node == Root || node.KeyCount >= minKeys)
            {
                return;
            }

            var rightNode = GetRightNode(node);

            if (rightNode != null && rightNode.KeyCount > minKeys)
            {
                LeftTurn(node, rightNode);
                return;
            }

            var leftNode = GetLeftNode(node);

            if (leftNode != null && leftNode.KeyCount > minKeys)
            {
                RightTurn(leftNode, node);
                return;
            }

            if (rightNode != null)
            {
                Merge(node, rightNode);
            }
            else
            {
                Merge(leftNode, node);
            }
        }

        private void Merge(BTreeNode<T> leftNode, BTreeNode<T> rightNode)
        {
            var separatorIndex = GetNextSeparatorIndex(leftNode);
            var parent = leftNode.Parent;

            var newNode = new BTreeNode<T>(maxKeys, leftNode.Parent);
            var newIndex = 0;

            for (var i = 0; i < leftNode.KeyCount; i++)
            {
                newNode.Keys[newIndex] = leftNode.Keys[i];

                if (leftNode.Children[i] != null)
                {
                    SetChild(newNode, newIndex, leftNode.Children[i]);
                }

                if (leftNode.Children[i + 1] != null)
                {
                    SetChild(newNode, newIndex + 1, leftNode.Children[i + 1]);
                }

                newIndex++;
            }

            if (leftNode.KeyCount == 0 && leftNode.Children[0] != null)
            {
                SetChild(newNode, newIndex, leftNode.Children[0]);
            }

            newNode.Keys[newIndex] = parent.Keys[separatorIndex];
            newIndex++;

            for (var i = 0; i < rightNode.KeyCount; i++)
            {
                newNode.Keys[newIndex] = rightNode.Keys[i];

                if (rightNode.Children[i] != null)
                {
                    SetChild(newNode, newIndex, rightNode.Children[i]);
                }

                if (rightNode.Children[i + 1] != null)
                {
                    SetChild(newNode, newIndex + 1, rightNode.Children[i + 1]);
                }

                newIndex++;
            }

            if (rightNode.KeyCount == 0 && rightNode.Children[0] != null)
            {
                SetChild(newNode, newIndex, rightNode.Children[0]);
            }

            newNode.KeyCount = newIndex;
            SetChild(parent, separatorIndex, newNode);
            RemoveAt(parent.Keys, separatorIndex);
            parent.KeyCount--;
            RemoveChild(parent, separatorIndex + 1);

            if (parent.KeyCount == 0 && parent == Root)
            {
                Root = newNode;
                Root.Parent = null;

                if (Root.KeyCount == 0)
                {
                    Root = null;
                }

                return;
            }

            if (parent.KeyCount < minKeys)
            {
                Balance(parent);
            }
        }

        private void RightTurn(BTreeNode<T> leftNode, BTreeNode<T> rightNode)
        {
            var parentIndex = GetNextSeparatorIndex(leftNode);
            InsertAt(rightNode.Keys, 0, rightNode.Parent.Keys[parentIndex]);
            rightNode.KeyCount++;
            InsertChild(rightNode, 0, leftNode.Children[leftNode.KeyCount]);
            rightNode.Parent.Keys[parentIndex] = leftNode.Keys[leftNode.KeyCount - 1];
            RemoveAt(leftNode.Keys, leftNode.KeyCount - 1);
            leftNode.KeyCount--;
            RemoveChild(leftNode, leftNode.KeyCount + 1);
        }

        private void LeftTurn(BTreeNode<T> leftNode, BTreeNode<T> rightNode)
        {
            var parentIndex = GetNextSeparatorIndex(leftNode);
            leftNode.Keys[leftNode.KeyCount] = leftNode.Parent.Keys[parentIndex];
            leftNode.KeyCount++;
            SetChild(leftNode, leftNode.KeyCount, rightNode.Children[0]);
            leftNode.Parent.Keys[parentIndex] = rightNode.Keys[0];
            RemoveAt(rightNode.Keys, 0);
            rightNode.KeyCount--;
            RemoveChild(rightNode, 0);
        }

        private int GetNextSeparatorIndex(BTreeNode<T> node)
        {
            var parent = node.Parent;

            if (node.Index == 0)
            {
                return 0;
            }

            if (node.Index == parent.KeyCount)
            {
                return node.Index - 1;
            }

            return node.Index;
        }

        private BTreeNode<T> GetRightNode(BTreeNode<T> node)
        {
            var parent = node.Parent;

            return node.Index == parent.KeyCount ? null : parent.Children[node.Index + 1];
        }

        private BTreeNode<T> GetLeftNode(BTreeNode<T> node)
        {
            return node.Index == 0 ? null : node.Parent.Children[node.Index - 1];
        }

        private void SetChild(BTreeNode<T> parent, int childIndex, BTreeNode<T> child)
        {
            parent.Children[childIndex] = child;

            if (child == null)
            {
                return;
            }

            child.Parent = parent;
            child.Index = childIndex;
        }

        private void InsertChild(BTreeNode<T> parent, int childIndex, BTreeNode<T> child)
        {
            InsertAt(parent.Children, childIndex, child);

            if (child != null)
            {
                child.Parent = parent;
            }

            for (var i = childIndex; i <= parent.KeyCount; i++)
            {
                if (parent.Children[i] != null)
                {
                    parent.Children[i].Index = i;
                }
            }
        }

        private void RemoveChild(BTreeNode<T> parent, int childIndex)
        {
            RemoveAt(parent.Children, childIndex);

            for (var i = childIndex; i <= parent.KeyCount; i++)
            {
                if (parent.Children[i] != null)
                {
                    parent.Children[i].Index = i;
                }

            }
        }

        private void InsertAt<S>(S[] array, int index, S value)
        {
            Array.Copy(array, index, array, index + 1, array.Length - index - 1);
            array[index] = value;
        }

        private void RemoveAt<S>(S[] array, int index)
        {
            Array.Copy(array, index + 1, array, index, array.Length - index - 1);
        }

        public List<T> InOrder(BTreeNode<T> node, List<T> SortedKeys)
        {
            if (node != null)
            {
                for (var i = 0; i < node.Children.Length; i++)
                {

                    InOrder(node.Children[i], SortedKeys);
                    if (i < node.Keys.Length) SortedKeys.Add(node.Keys[i]);
                }
            }

            return SortedKeys;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BStarTreeEnumerator<T>(Root);
        }
    }

    public abstract class BNode<T> where T : IComparable
    {
        internal int Index;
        internal T[] Keys { get; set; }
        internal int KeyCount;
        internal abstract BNode<T> GetParent();
        internal abstract BNode<T>[] GetChildren();

        internal BNode(int maxKeys)
        {
            Keys = new T[maxKeys];
        }

        internal int GetHalfIndex()
        {
            return (KeyCount / 2) + 1;
        }
    }

    public class BTreeNode<T> : BNode<T> where T : IComparable
    {
        internal BTreeNode<T> Parent { get; set; }
        internal BTreeNode<T>[] Children { get; set; }
        internal bool IsLeaf => Children[0] == null;

        internal BTreeNode(int maxKeys, BTreeNode<T> parent) : base(maxKeys)
        {
            Parent = parent;
            Children = new BTreeNode<T>[maxKeys + 1];
        }

        internal override BNode<T> GetParent()
        {
            return Parent;
        }

        internal override BNode<T>[] GetChildren()
        {
            return Children;
        }
    }

    internal class BStarTreeEnumerator<T> : IEnumerator<T> where T : IComparable
    {
        private readonly BTreeNode<T> Root;
        private Stack<BTreeNode<T>> Progress;
        private BTreeNode<T> current;
        private int Index;

        internal BStarTreeEnumerator(BTreeNode<T> root)
        {
            Root = root;
        }

        public bool MoveNext()
        {
            if (Root == null)
            {
                return false;
            }

            if (Progress == null)
            {
                current = Root;
                Progress = new Stack<BTreeNode<T>>(Root.Children.Take(Root.KeyCount + 1).Where(x => x != null));
                return current.KeyCount > 0;
            }

            if (current != null && Index + 1 < current.KeyCount)
            {
                Index++;
                return true;
            }

            if (Progress.Count > 0)
            {
                Index = 0;

                current = Progress.Pop();

                foreach (var child in current.Children.Take(current.KeyCount + 1).Where(x => x != null))
                {
                    Progress.Push(child);
                }

                return true;
            }

            return false;
        }

        public void Reset()
        {
            Progress = null;
            current = null;
            Index = 0;
        }

        object IEnumerator.Current => Current;

        public T Current
        {
            get
            {
                return current.Keys[Index];
            }
        }

        public void Dispose()
        {
            Progress = null;
        }
    }
}
