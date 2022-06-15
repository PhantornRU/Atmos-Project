using System.Collections;
using UnityEngine;

/// <summary>
/// Base for all interactable component interfaces. It's basically a marker interface (usually something
/// best avoided) which is only used
/// because of C#'s lack of default interface methods.
/// </summary>
public interface IInteractable<T>
	where T : Interaction
{
	/// <summary>
	/// Run the client-side interaction logic
	/// </summary>
	/// <param name="interaction"></param>
	/// <returns>true if further interaction checking should stop (typically you would return true if something
	/// actually happened).</returns>
	bool Interact(T interaction);
}
