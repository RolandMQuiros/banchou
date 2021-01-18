using System;
using NUnit.Framework;

using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Banchou.Test {
    public class ReactivePropertyTests {
        public class TestState {
            public class Substate {
                public Vector3 Position;
                public Vector3 Forward;
            }

            public ReactiveProperty<int> IntProperty = new ReactiveProperty<int>();
            public ReactiveCollection<int> SetProperty = new ReactiveCollection<int>();
            public ReactiveDictionary<int, Substate> DictProperty = new ReactiveDictionary<int, Substate>();
        }

        [Test]
        public void IntPropertyGarbageGenerated() {
            var state = new TestState();
            var startGC = GC.GetTotalMemory(false);
            state.IntProperty.Value = 1;
            state.IntProperty.Value = 2;
            state.IntProperty.Value = 3;
            state.IntProperty.Value = 4;
            state.IntProperty.Value = 5;
            var endGC = GC.GetTotalMemory(false);
            Assert.AreEqual(startGC, endGC);
        }

        [Test]
        public void SetPropertyGarbageGenerated() {
            var state = new TestState();

            var startGC = GC.GetTotalMemory(false);
            state.SetProperty.Add(1);
            state.SetProperty.Add(2);
            state.SetProperty.Add(3);
            state.SetProperty.Add(4);
            state.SetProperty.Add(5);
            state.SetProperty.Add(6);
            state.SetProperty.Add(7);
            state.SetProperty.Add(8);
            var endGC = GC.GetTotalMemory(false);
            Assert.AreEqual(startGC, endGC);
        }
    }
}