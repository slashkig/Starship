using Extensions.Enums;
using SpaceGame;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SpaceGame
{
    /// <summary>Represents the current state of the game.</summary>
    public enum Mode
    {
        Paused,
        Playing,
        Map,
        Build
    }

    public enum IterBehavior
    {
        DontCheckForFail,
        EnsureSuccessBeforeRunning,
        SkipFails,
        RunUntilFail
    }

    public enum Health
    {
        Hull,
        Armor,
        Shields
    }

    public enum Damage
    {
        Kinetic,
        Energy,
        Explosive
    }

    public enum Resource
    {
        Power,
        Mineral,
        Ore,
        Metal,
        Alloy,
        Fuel
    }

    public enum Ammo
    {
        Missile,
        Shell,
        Cell
    }

    public static class GameMethods
    {
        public static GameManager Manager { get; } = Object.FindObjectOfType<GameManager>();
        public static ObjectManager ObjManager { get; } = Object.FindObjectOfType<ObjectManager>();
        /// <summary>The resources required to manufacture a product.</summary>
        public static readonly Dictionary<Enum, Dictionary<Resource, float>> recipies = new Dictionary<Enum, Dictionary<Resource, float>>()
        {
            [Resource.Power] = new Dictionary<Resource, float>() { [Resource.Mineral] = 0.6f },
            [Resource.Metal] = new Dictionary<Resource, float>() { [Resource.Ore] = 2 },
            [Resource.Alloy] = new Dictionary<Resource, float>() { [Resource.Metal] = 2 },
            [Resource.Fuel] = new Dictionary<Resource, float>() { [Resource.Mineral] = 2 },
            [Ammo.Cell] = new Dictionary<Resource, float>() { [Resource.Power] = 2, [Resource.Mineral] = 1 },
            [Ammo.Missile] = new Dictionary<Resource, float>() { [Resource.Metal] = 2, [Resource.Mineral] = 2, [Resource.Fuel] = 1.5f },
            [Ammo.Shell] = new Dictionary<Resource, float>() { [Resource.Metal] = 1.5f, [Resource.Mineral] = 1 }
        };
        /// <summary>The strength of each damage type against each health type.</summary>
        public static readonly Dictionary<Damage, Dictionary<Health, float>> damageMods = new Dictionary<Damage, Dictionary<Health, float>>()
        {
            [Damage.Kinetic] = new Dictionary<Health, float>()
            {
                [Health.Shields] = 1.5f,
                [Health.Armor] = 0.5f,
                [Health.Hull] = 1
            },
            [Damage.Energy] = new Dictionary<Health, float>()
            {
                [Health.Shields] = 0.5f,
                [Health.Armor] = 1.5f,
                [Health.Hull] = 1
            },
            [Damage.Explosive] = new Dictionary<Health, float>()
            {
                [Health.Shields] = 0.3f,
                [Health.Armor] = 0.3f,
                [Health.Hull] = 2
            }
        };
        /// <summary>The resource names used in-game.</summary>
        public static readonly Dictionary<Enum, string> names = new Dictionary<Enum, string>()
        {
            [Resource.Alloy] = "Alloys",
            [Resource.Power] = "Energy",
            [Resource.Fuel] = "Fuel",
            [Resource.Metal] = "Metals",
            [Resource.Mineral] = "Minerals",
            [Resource.Ore] = "Ore",
            [Ammo.Cell] = "Energy Cells",
            [Ammo.Missile] = "Missiles",
            [Ammo.Shell] = "Shells"
        };
        /// <summary>Returns a ConstraintSource using the specified transform and a weight of 1.</summary>
        /// <param name="transform">The source transform to use.</param>
        public static ConstraintSource AimSource(Transform transform) => new ConstraintSource
        {
            sourceTransform = transform,
            weight = 1
        };
        /// <summary>Converts a world position to the position of a filled tile using an edge.</summary>
        /// <param name="edge">The edge to reflect worldPos off of if it's not on a filled cell.</param>
        /// <param name="worldPos">The world position to start at.</param>
        /// <returns>The resulting cell position, or an exception if no filled tile is found.</returns>
        public static Vector3Int EdgeToCell(this Tilemap tmap, Vector3 edge, Vector3 worldPos)
        {
            Vector3 adjustedPos;
            return tmap.HasCell(worldPos) ? tmap.WorldToCell(worldPos) : tmap.HasCell(adjustedPos = edge + (edge - worldPos)) ?
                tmap.WorldToCell(adjustedPos) : throw new NoTilePresentException("No tile on specified edge");
        }
        /// <summary>Returns whether there is a tile at the world position.</summary>
        /// <param name="worldPos">The world position to check.</param>
        public static bool HasCell(this Tilemap tilemap, Vector3 worldPos) => tilemap.HasTile(tilemap.WorldToCell(worldPos));

        public static List<TileBase> GetAdjacentTiles(this Tilemap tilemap, Vector3Int position)
        {
            List<TileBase> tiles = new List<TileBase>();
            foreach (Vector3Int adjacent in new Vector3Int[]{ Vector3Int.left, Vector3Int.right, Vector3Int.down, Vector3Int.up })
            {
                if (tilemap.HasTile(position + adjacent)) tiles.Add(tilemap.GetTile(position + adjacent));
            }
            return tiles;
        }
    }

    public interface IDamageable : IDestroyable
    {
        public int Id { get; set; }

        public Dictionary<Health, float> Health { get; set; }

        public void TakeDamage(float damage, Damage damageType);
    }

    public interface IDestroyable
    {
        public void Die();
    }
}

namespace UnityEngine.Tilemaps
{
    public class NoTilePresentException : UnityException
    {
        public NoTilePresentException() { }

        public NoTilePresentException(string message) : base(message) { }

        public NoTilePresentException(string message, Exception inner) : base(message, inner) { }

        protected NoTilePresentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

namespace Extensions
{
    public static class Toolbox
    {
        /// <summary>Returns a random float within the specified bound.</summary>
        /// <param name="bound">The maximum distance from zero of the return value.</param>
        public static float Range(float bound) => Random.Range(-bound, bound);
        /// <summary>Rounds a float to the nearest tenth.</summary>
        /// <param name="number">The float to round.</param>
        /// <returns>The rounded float.</returns>
        public static float RoundToTenths(float number) => Mathf.Round(10 * number) / 10;
        /// <summary>Applies a comparison to each element of a dictionary.</summary>
        /// <param name="dict">The dictionary to apply the comparison to.</param>
        /// <param name="Constraint">The constraint to test for.</param>
        /// <returns>Whether the comparison was true for all elements of the dictionary.</returns>
        public static bool TrueForAll<K, V>(this Dictionary<K, V> dict, Func<K, V, bool> Constraint)
        {
            foreach (KeyValuePair<K, V> valuePair in dict)
            {
                if (!Constraint(valuePair.Key, valuePair.Value)) return false;
            }
            return true;
        }
        /// <summary>Iterates through a list </summary>
        /// <param name="list">The list to iterate through.</param>
        /// <param name="Action">The action to take if an element succeeds the conditional check.</param>
        /// <param name="Condition">The condition to test for.</param>
        /// <param name="failBehavior">How fails should be managed.</param>
        /// <returns>Whether the action was successful for all elements of the list.</returns>
        public static bool IterThrough<T>(this List<T> list, Action<T> Action, Predicate<T> Condition = null, IterBehavior failBehavior = IterBehavior.DontCheckForFail)
        {
            switch (failBehavior)
            {
                case IterBehavior.DontCheckForFail:
                    list.ForEach(Action);
                    return true;
                case IterBehavior.EnsureSuccessBeforeRunning:
                    if (list.TrueForAll(Condition)) goto case 0;
                    else return false;
                case IterBehavior.SkipFails:
                    bool success = true;
                    list.ForEach((T item) => {
                        if (Condition(item)) Action(item);
                        else if (success) success = false;
                    });
                    return success;
                case IterBehavior.RunUntilFail:
                    return list.TrueForAll((T item) => {
                        if (Condition(item)) { Action(item); return true; }
                        else { return false; }
                    });
            }
            return false;
        }

        public static List<EnumPair<E>> ToList<E>(this Dictionary<E, float> dict) where E : Enum
        {
            List<EnumPair<E>> list = new List<EnumPair<E>>(dict.Count);
            foreach(KeyValuePair<E, float> pair in dict)
            {
                list.Add(new EnumPair<E>(pair.Key, pair.Value));
            }
            return list;
        }
    }

    public static class TransformMethods
    {
        public static float Distance(this Transform transform, Transform target) =>
            Vector3.Distance(transform.position, target.position);

        public static void MatchRotation(this Transform transform, Transform direction, float maxDegrees) =>
            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction.rotation, maxDegrees);

        public static float AngularDifference(this Transform transform, Transform target) =>
            Quaternion.Angle(transform.rotation, target.rotation);
        
        /// <summary> Returns a random point within the bounds of a collider. </summary>
        public static Vector3 RandPos(this Transform transform, Collider2D collider)
        {
            Vector3 position;
            while (true)
            {
                if (collider.OverlapPoint(transform.position + (position = new Vector3(Toolbox.Range(transform.lossyScale.x),
                    Toolbox.Range(transform.lossyScale.y))))) { return position; }
            }
        }

        /// <summary> Returns a random point within the bounds of a rectangle. </summary>
        public static Vector3 RandPos(this Transform transform, float xSize, float ySize) => new Vector3(Toolbox.Range(xSize / 2),
            Toolbox.Range(ySize / 2));
    }

    namespace Enums
    {
        /// <summary>An enum linked to a value.</summary>
        /// <typeparam name="E">The type of the enum.</typeparam>
        [Serializable]
        public struct EnumPair<E> where E : Enum
        {
            public E type;
            public float value;

            public static implicit operator E(EnumPair<E> enumPair) => enumPair.type;
            public static implicit operator float(EnumPair<E> enumPair) => enumPair.value;
            public static implicit operator EnumPair<Enum>(EnumPair<E> pair) => new EnumPair<Enum>(pair.type, pair.value);
            public static implicit operator KeyValuePair<E, float>(EnumPair<E> enumPair) =>
                new KeyValuePair<E, float>(enumPair.type, enumPair.value);
            public static implicit operator EnumPair<E>(KeyValuePair<E, float> pair) => new EnumPair<E>(pair.Key, pair.Value);
            public static implicit operator (E, float)(EnumPair<E> enumPair) => (enumPair.type, enumPair.value);
            public static implicit operator EnumPair<E>((E type, float value) pair) => new EnumPair<E>(pair.type, pair.value);
            public static EnumPair<E> operator +(EnumPair<E> left, EnumPair<E> right) => left += right.value;
            public static EnumPair<E> operator -(EnumPair<E> left, EnumPair<E> right) => left -= right.value;
            public static EnumPair<E> operator +(EnumPair<E> left, float right) { left.value += right; return left; }
            public static EnumPair<E> operator -(EnumPair<E> left, float right) { left.value -= right; return left; }
            public static EnumPair<E> operator *(EnumPair<E> left, float right) { left.value *= right; return left; }
            public static EnumPair<E> operator /(EnumPair<E> left, float right) { left.value /= right; return left; }
            public static EnumPair<E> operator +(float left, EnumPair<E> right) => right + left;
            public static EnumPair<E> operator -(float left, EnumPair<E> right) => right - left;
            public static EnumPair<E> operator *(float left, EnumPair<E> right) => right * left;
            public static EnumPair<E> operator /(float left, EnumPair<E> right) => right / left;

            public EnumPair(E type, float value)
            {
                this.type = type;
                this.value = value;
            }

            public (E, float) Split() => (type, value);
        }

        [Serializable]
        public struct Thing
        {
            public Resource enumtype;
            public float number;
        }

        /// <summary>An enum 'key' linked to a list of EnumPairs.</summary>
        /// <typeparam name="EK">The type of the enum key.</typeparam>
        /// <typeparam name="EV">The type of the enums in the pair list.</typeparam>
        [Serializable]
        public struct PairList<EK, EV> where EK : Enum where EV : Enum
        {
            public EK key;
            public List<EnumPair<EV>> pairs;
        }

        public static class EnumMethods
        {
            /// <summary>Adds the contents of an Enum-float dictionary to another dictionary using the same enum.</summary>
            /// <param name="pairs">The dictionary to add.</param>
            /// <param name="dict">The dictionary to add to.</param>
            public static void AddToDict<E>(this Dictionary<E, float> dict, Dictionary<E, float> pairs) where E : Enum
            {
                foreach (EnumPair<E> pair in pairs) { AddToDict(pair, ref dict); }
            }
            /// <summary>Adds the contents of a list of EnumPairs to a dictionary using the same enum.</summary>
            /// <param name="enumPairs">The EnumPairs to add.</param>
            /// <param name="dict">The enum/number dictionary to add to.</param>
            public static void AddToDict<T, E>(List<EnumPair<T>> enumPairs, ref Dictionary<E, float> dict) where E : Enum where T : E
            {
                foreach (EnumPair<T> pair in enumPairs) { AddToDict(pair, ref dict); }
            }
            /// <summary>Adds the enum and float of an EnumPair to a dictionary using the same enum.</summary>
            /// <param name="enumPair">The EnumPair containing the enum key and float value to add.</param>
            /// <param name="dict">The enum/number dictionary to add to.</param>
            public static void AddToDict<T, E>(EnumPair<T> enumPair, ref Dictionary<E, float> dict) where E : Enum where T : E
            {
                if (dict.ContainsKey(enumPair.type)) dict[enumPair.type] += enumPair.value;
                else dict.Add(enumPair.type, enumPair.value);
            }
        }
    }
}