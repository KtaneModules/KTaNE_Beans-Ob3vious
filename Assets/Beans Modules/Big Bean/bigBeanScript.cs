using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class bigBeanScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable Bean;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int bean = 0;
	private float offset;
	private int timeoffset;
	private int[][] colours = new int[][] { new int[] { 192, 192, 0 }, new int[] { 84, 144, 192 }, new int[] { 0, 0, 0 } };
	private int eatensteps = 0;
	private int set;
	private List<int> steps = new List<int> { };

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed()
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			switch (eatensteps)
            {
				case 0:
					Bean.transform.localScale = new Vector3(0.075f, 0.075f, 0.12f);
					break;
				case 1:
					Bean.transform.localScale = new Vector3(0.05f, 0.05f, 0.08f);
					break;
				case 2:
					Bean.transform.localScale = new Vector3(0f, 0f, 0f);
					break;
			}
			eatensteps++;
			if (set == 0)
			{
				set++;
				StartCoroutine(Timer());
			}
			else
				set++;
			return false;
		};
	}

	private void BeanHovered()
	{
		string[] colour = { "orange", "yellow", "green" };
		Text.GetComponent<TextMesh>().text = colour[bean % 3];
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	void Awake()
	{
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";

		Bean.OnInteract += BeanPressed();
		Bean.OnHighlight += delegate { BeanHovered(); return; };
		Bean.OnHighlightEnded += delegate { BeanHoverEnded(); return; };
	}

	void Start () {

		offset = Rnd.Range(0f, 360f);
		timeoffset = Rnd.Range(0, 100);
		bean = Rnd.Range(0, 6);
		List<int> goal = new List<int> { };
		switch (bean)
		{
			case 0:
			case 5:
				goal = new List<int> { 1, 2 };
				break;
			case 1:
				goal = new List<int> { 1, 1, 1 };
				break;
			case 2:
			case 3:
				goal = new List<int> { 2, 1 };
				break;
			case 4:
				goal = new List<int> { 3 };
				break;
		}
		Debug.LogFormat("[Big Bean #{0}] The bean is: {1}.", _moduleID, "oyg"[bean % 3].ToString() + (bean >= 3 ? "w" : ""));
		Debug.LogFormat("[Big Bean #{0}] The set to take a bite of the bean in is [{1}].", _moduleID, goal.Join());
		Bean.GetComponent<MeshRenderer>().material.color = new Color(colours[0][bean % 3] / 255f, colours[1][bean % 3] / 255f, colours[2][bean % 3] / 255f);
		StartCoroutine(Wobble());
	}

	private IEnumerator Wobble()
	{
		float t = 0;
		while (true)
		{
			if (bean / 3 == 1)
				Bean.transform.localEulerAngles = new Vector3(0f, Mathf.Sin(t + timeoffset) * 15f + offset, 0f);
			else
				Bean.transform.localEulerAngles = new Vector3(0f, offset, 0f);
			t += 0.125f;
			yield return new WaitForSeconds(0.02f);
		}
	}

	private IEnumerator Timer()
	{
		yield return new WaitForSeconds(1f);
		steps.Add(set);
		set = 0;
		if (eatensteps == 3)
		{
			List<int> goal = new List<int> { };
			switch (bean)
			{
				case 0:
				case 5:
					goal = new List<int> { 1, 2 };
					break;
				case 1:
					goal = new List<int> { 1, 1, 1 };
					break;
				case 2:
				case 3:
					goal = new List<int> { 2, 1 };
					break;
				case 4:
					goal = new List<int> { 3 };
					break;
			}
			if (steps.Join() != goal.Join())
			{
				Debug.LogFormat("[Big Bean #{0}] You submitted [{1}], but I expected [{2}].", _moduleID, steps.Join(), goal.Join());
				Module.HandleStrike();
				StartCoroutine(Strike());
			}
			else
				Debug.LogFormat("[Big Bean #{0}] Submission [{1}] was the correct submission.", _moduleID, steps.Join());
			Module.HandlePass();
			Solve();
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatensteps < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
	}

	private IEnumerator Highlight()
	{
		Bean.OnHighlight();
		yield return new WaitForSeconds(0.9f);
		Bean.OnHighlightEnded();
		yield return new WaitForSeconds(0.1f);
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} highlight' to highlight the bean, '!{0} 2 1' to eat the bean twice, then once. Note that you cannot press it more than 3 times.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "highlight")
			StartCoroutine(Highlight());
		else
		{
			string validCommands = "123";
			command = command.Replace(" ", "");
			for (int i = 0; i < command.Length; i++)
				if (!validCommands.Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			yield return "strike";
			yield return "solve";
			for (int i = 0; eatensteps != 3 && i < command.Length; i++)
			{
				yield return null;
				for (int j = 0; j < command[i] - '0' && eatensteps != 3; j++)
				{
					Bean.OnInteract();
					yield return null;
				}
				yield return new WaitForSeconds(1f);
			}
		}
		yield return null;
	}
	
	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		List<int> goal = new List<int> { };
		switch (bean)
		{
			case 0:
			case 5:
				goal = new List<int> { 1, 2 };
				break;
			case 1:
				goal = new List<int> { 1, 1, 1 };
				break;
			case 2:
			case 3:
				goal = new List<int> { 2, 1 };
				break;
			case 4:
				goal = new List<int> { 3 };
				break;
		}
		for (int i = 0; eatensteps != 3 && i < goal.Count(); i++)
		{
			while (set != 0)
				yield return true;
			for (int j = 0; j < goal[i]; j++)
			{
				Bean.OnInteract();
				yield return null;
			}
		}
	}
}
