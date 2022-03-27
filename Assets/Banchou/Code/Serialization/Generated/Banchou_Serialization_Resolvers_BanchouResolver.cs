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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(32)
            {
                { typeof(global::System.Collections.Generic.Dictionary<int, bool>), 0 },
                { typeof(global::System.Collections.Generic.Dictionary<int, float>), 1 },
                { typeof(global::System.Collections.Generic.Dictionary<int, global::Banchou.Pawn.PawnState>), 2 },
                { typeof(global::System.Collections.Generic.Dictionary<int, global::Banchou.Player.PlayerState>), 3 },
                { typeof(global::System.Collections.Generic.Dictionary<int, int>), 4 },
                { typeof(global::System.Collections.Generic.List<string>), 5 },
                { typeof(global::Banchou.Combatant.AttackPhase), 6 },
                { typeof(global::Banchou.Combatant.CombatantTeam), 7 },
                { typeof(global::Banchou.Combatant.GrabbedPose), 8 },
                { typeof(global::Banchou.Combatant.GrabPhase), 9 },
                { typeof(global::Banchou.Combatant.GuardStyle), 10 },
                { typeof(global::Banchou.Combatant.HitStyle), 11 },
                { typeof(global::Banchou.Pawn.PawnSpatial.MovementStyle), 12 },
                { typeof(global::Banchou.Player.InputCommand), 13 },
                { typeof(global::Banchou.Board.BoardState), 14 },
                { typeof(global::Banchou.Combatant.AttackState), 15 },
                { typeof(global::Banchou.Combatant.CombatantState), 16 },
                { typeof(global::Banchou.Combatant.CombatantStats), 17 },
                { typeof(global::Banchou.Combatant.DefensiveState), 18 },
                { typeof(global::Banchou.Combatant.GrabState), 19 },
                { typeof(global::Banchou.Combatant.HitState), 20 },
                { typeof(global::Banchou.GameState), 21 },
                { typeof(global::Banchou.Network.Message.ConnectClient), 22 },
                { typeof(global::Banchou.Network.Message.Connected), 23 },
                { typeof(global::Banchou.Network.Message.TimeRequest), 24 },
                { typeof(global::Banchou.Network.Message.TimeResponse), 25 },
                { typeof(global::Banchou.Pawn.PawnAnimatorFrame), 26 },
                { typeof(global::Banchou.Pawn.PawnSpatial), 27 },
                { typeof(global::Banchou.Pawn.PawnState), 28 },
                { typeof(global::Banchou.Player.PlayerInputState), 29 },
                { typeof(global::Banchou.Player.PlayersState), 30 },
                { typeof(global::Banchou.Player.PlayerState), 31 },
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
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<int, bool>();
                case 1: return new global::MessagePack.Formatters.DictionaryFormatter<int, float>();
                case 2: return new global::MessagePack.Formatters.DictionaryFormatter<int, global::Banchou.Pawn.PawnState>();
                case 3: return new global::MessagePack.Formatters.DictionaryFormatter<int, global::Banchou.Player.PlayerState>();
                case 4: return new global::MessagePack.Formatters.DictionaryFormatter<int, int>();
                case 5: return new global::MessagePack.Formatters.ListFormatter<string>();
                case 6: return new Banchou.Serialization.Formatters.Banchou.Combatant.AttackPhaseFormatter();
                case 7: return new Banchou.Serialization.Formatters.Banchou.Combatant.CombatantTeamFormatter();
                case 8: return new Banchou.Serialization.Formatters.Banchou.Combatant.GrabbedPoseFormatter();
                case 9: return new Banchou.Serialization.Formatters.Banchou.Combatant.GrabPhaseFormatter();
                case 10: return new Banchou.Serialization.Formatters.Banchou.Combatant.GuardStyleFormatter();
                case 11: return new Banchou.Serialization.Formatters.Banchou.Combatant.HitStyleFormatter();
                case 12: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnSpatial_MovementStyleFormatter();
                case 13: return new Banchou.Serialization.Formatters.Banchou.Player.InputCommandFormatter();
                case 14: return new Banchou.Serialization.Formatters.Banchou.Board.BoardStateFormatter();
                case 15: return new Banchou.Serialization.Formatters.Banchou.Combatant.AttackStateFormatter();
                case 16: return new Banchou.Serialization.Formatters.Banchou.Combatant.CombatantStateFormatter();
                case 17: return new Banchou.Serialization.Formatters.Banchou.Combatant.CombatantStatsFormatter();
                case 18: return new Banchou.Serialization.Formatters.Banchou.Combatant.DefensiveStateFormatter();
                case 19: return new Banchou.Serialization.Formatters.Banchou.Combatant.GrabStateFormatter();
                case 20: return new Banchou.Serialization.Formatters.Banchou.Combatant.HitStateFormatter();
                case 21: return new Banchou.Serialization.Formatters.Banchou.GameStateFormatter();
                case 22: return new Banchou.Serialization.Formatters.Banchou.Network.Message.ConnectClientFormatter();
                case 23: return new Banchou.Serialization.Formatters.Banchou.Network.Message.ConnectedFormatter();
                case 24: return new Banchou.Serialization.Formatters.Banchou.Network.Message.TimeRequestFormatter();
                case 25: return new Banchou.Serialization.Formatters.Banchou.Network.Message.TimeResponseFormatter();
                case 26: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnAnimatorFrameFormatter();
                case 27: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnSpatialFormatter();
                case 28: return new Banchou.Serialization.Formatters.Banchou.Pawn.PawnStateFormatter();
                case 29: return new Banchou.Serialization.Formatters.Banchou.Player.PlayerInputStateFormatter();
                case 30: return new Banchou.Serialization.Formatters.Banchou.Player.PlayersStateFormatter();
                case 31: return new Banchou.Serialization.Formatters.Banchou.Player.PlayerStateFormatter();
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
