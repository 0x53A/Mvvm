﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public class Node<T> : IEnumerable<T>
    {
        public List<Node<T>> Children { get; private set; }
        public Node<T> Parent { get; private set; }
        public T Value { get; set; }

        public Node()
        {
            Children = new List<Node<T>>();
        }

        public void AddNode(Node<T> node)
        {
            Contract.Requires(node != null);
            Contract.Requires(node.Parent == null);
            Children.Add(node);
            node.Parent = this;
        }

        public Node<T> AddChild(T val)
        {
            var node = new Node<T>();
            node.Value = val;
            AddNode(node);
            return node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Flatten((n) => n.Children).Select(n => n.Value).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}