// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace Banchou.Serialization.Resolvers
{
    using System;

    public class BanchouResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new BanchouResolver();

        private BanchouResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = BanchouResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class BanchouResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static BanchouResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(18)
            {
                { typeof(global::Banchou.Network.Message.PayloadType), 0 },
                { typeof(global::Banchou.Player.InputCommand), 1 },
                { typeof(global::Banchou.Board.BoardState), 2 },
                { typeof(global::Banchou.GameState), 3 },
                { typeof(global::Banchou.Network.Message.ConnectClient), 4 },
                { typeof(global::Banchou.Network.Message.Connected), 5 },
                { typeof(global::Banchou.Network.Message.Envelope), 6 },
                { typeof(global::Banchou.Network.Message.TimeRequest), 7 },
                { typeof(global::Banchou.Network.Message.TimeResponse), 8 },
                { typeof(global::Banchou.Network.NetworkState), 9 },
                { typeof(global::Banchou.Pawn.FrameData), 10 },
                { typeof(global::Banchou.Pawn.PawnHistory), 11 },
                { typeof(global::Banchou.Pawn.PawnSpatial), 12 },
                { typeof(global::Banchou.Pawn.PawnState), 13 },
                { typeof(global::Banchou.Player.InputUnit), 14 },
                { typeof(global::Banchou.Player.PlayerInputStates), 15 },
                { typeof(global::Banchou.Player.PlayersState), 16 },
                { typeof(global::Banchou.Player.PlayerState), 17 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new Banchou.Serialization.Formatters.Banchou.Network.Message.PayloadTypeFormatter();
                case 1: return new Banchou.Serialization.Formatters.Banchou.Player.InputCommandFormatter();
                case 2: return new Banchou.Serialization.Formatters.Banchou.Board.BoardStateFormatter();
                case 3: return new Banchou.Serialization.Formatters.Banchou.GameStateFormatter();
                case 4: return new Banchou.Serialization.Formatters.Banchou.Network.Message.ConnectClientFormatter();
                case 5: return new Banchou.Serialization.Formatters.Banchou.Network.Message.ConnectedFormatter();
                case 6: return new Banchou.Serialization.Formatters.Banchou.Network.Message.EnvelopeFormatter();
                case 7: return new Banchou.Serialization.Formatters.Banchou.Network.Message.TimeRequestFormatter();
                case 8: return new Banchou.Serialization.Formatters.Banchou.Network.Message.TimeResponseFormatter();
                case 9: return new Banchou.Serialization.Formatters.Banchou.Network.NetworkStateFormatter();
                case 10: return new Banchou.Serialization.Formatters.Banchou.Pawn.FrameDataFormatter();
                case 11: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnHistoryFormatter();
                case 12: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnSpatialFormatter();
                case 13: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnStateFormatter();
                case 14: return new Banchou.Serialization.Formatters.Banchou.Player.InputUnitFormatter();
                case 15: return new Banchou.Serialization.Formatters.Banchou.Player.PlayerInputStatesFormatter();
                case 16: return new Banchou.Serialization.Formatters.Banchou.Player.PlayersStateFormatter();
                case 17: return new Banchou.Serialization.Formatters.Banchou.Player.PlayerStateFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1649 // File name should match first type name
