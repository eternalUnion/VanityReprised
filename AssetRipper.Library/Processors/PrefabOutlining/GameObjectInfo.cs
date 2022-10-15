﻿using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Core.Linq;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Component_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Transform_;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AssetRipper.Library.Processors.PrefabOutlining
{
	internal sealed class GameObjectInfo : IEquatable<GameObjectInfo>
	{
		public GameObjectInfo(ImmutableArray<GameObjectInfo?> children, ImmutableArray<int> components)
		{
			Children = children;
			Components = components;
			Hash = CalculateHash(children, components);
		}

		public ImmutableArray<GameObjectInfo?> Children { get; }
		public ImmutableArray<int> Components { get; }
		private int Hash { get; }

		public override string ToString()
		{
			return $"Children: {Children.Length} Components: {Components.Length}";
		}

		public override bool Equals(object? obj)
		{
			return obj is GameObjectInfo info && Equals(info);
		}

		public bool Equals(GameObjectInfo? other)
		{
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
			return other is not null
				&& Hash == other.Hash
				&& Components.AsSpan().SequenceEqual(other.Components.AsSpan())
				&& Children.AsSpan().SequenceEqual(other.Children.AsSpan());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
		}

		public override int GetHashCode()
		{
			return Hash;
		}

		public static bool operator ==(GameObjectInfo left, GameObjectInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GameObjectInfo left, GameObjectInfo right)
		{
			return !(left == right);
		}

		private static int CalculateHash(ImmutableArray<GameObjectInfo?> children, ImmutableArray<int> components)
		{
			HashCode hashCode = new();
			for (int i = 0; i < components.Length; i++)
			{
				hashCode.Add(components[i]);
			}
			for (int i = 0; i < children.Length; i++)
			{
				hashCode.Add(children[i]);
			}
			return hashCode.ToHashCode();
		}

		public static GameObjectInfo FromGameObject(IGameObject root)
		{
			GetTransformAndComponentArray(root, out ITransform? transform, out int[] components);

			GameObjectInfo?[] children;
			if (transform is null)
			{
				children = Array.Empty<GameObjectInfo?>();
			}
			else
			{
				PPtrAccessList<IPPtr_Transform_, ITransform> childList = transform.Children_C4P;
				if (childList.Count == 0)
				{
					children = Array.Empty<GameObjectInfo?>();
				}
				else
				{
					children = new GameObjectInfo?[childList.Count];
					for (int i = 0; i < childList.Count; i++)
					{
						IGameObject? child = childList[i]?.GameObject_C4P;
						children[i] = child is null ? null : FromGameObject(child);
					}
				}
			}

			return new GameObjectInfo(ImmutableArray.Create(children), ImmutableArray.Create(components));
		}

		public static void AddCollectionToDictionary(AssetCollection collection, Dictionary<IGameObject, GameObjectInfo> dictionary)
		{
			foreach (IGameObject gameObject in collection.SelectType<IUnityObjectBase, IGameObject>())
			{
				AddHierarchyToDictionary(gameObject, dictionary);
			}
		}

		public static GameObjectInfo AddHierarchyToDictionary(IGameObject root, Dictionary<IGameObject, GameObjectInfo> dictionary)
		{
			if (dictionary.TryGetValue(root, out GameObjectInfo? info))
			{
				return info;
			}

			GetTransformAndComponentArray(root, out ITransform? transform, out int[] components);

			GameObjectInfo?[] children;
			if (transform is null)
			{
				children = Array.Empty<GameObjectInfo?>();
			}
			else
			{
				PPtrAccessList<IPPtr_Transform_, ITransform> childList = transform.Children_C4P;
				if (childList.Count == 0)
				{
					children = Array.Empty<GameObjectInfo?>();
				}
				else
				{
					children = new GameObjectInfo?[childList.Count];
					for (int i = 0; i < childList.Count; i++)
					{
						IGameObject? child = childList[i]?.GameObject_C4P;
						children[i] = child is null ? null : AddHierarchyToDictionary(child, dictionary);
					}
				}
			}

			info = new GameObjectInfo(ImmutableArray.Create(children), ImmutableArray.Create(components));
			dictionary.Add(root, info);
			return info;
		}

		private static void GetTransformAndComponentArray(IGameObject root, out ITransform? transform, out int[] components)
		{
			transform = null;
			PPtrAccessList<IPPtr_Component_, IComponent> componentList = root.GetComponentAccessList();
			if (componentList.Count == 0)
			{
				components = Array.Empty<int>();
			}
			else
			{
				components = new int[componentList.Count];
				for (int i = 0; i < componentList.Count; i++)
				{
					IComponent? component = componentList[i];
					if (component is not null)
					{
						components[i] = component.ClassID;
						if (component is ITransform t)
						{
							transform = t;
						}
					}
					else
					{
						components[i] = -1;
					}
				}
			}
		}
	}
}
