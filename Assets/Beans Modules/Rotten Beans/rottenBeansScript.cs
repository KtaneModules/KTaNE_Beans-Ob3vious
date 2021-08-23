using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class rottenBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[][] colours = new int[][] { new int[] { 128, 64 }, new int[] { 64, 64 }, new int[] { 0, 64 } };
	private bool[] beansafe = new bool[9];
	private int[] referArray = { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
	private int eatenbeans = 0;
	private bool striked = false;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed(int pos)
	{
		return delegate
		{
			if (eatenbeans == 0)
                for (int i = 0; i < 9; i++)
					Beans[i].GetComponent<MeshRenderer>().material.color = new Color(96 / 255f, 64 / 255f, 32 / 255f);
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			if (eatenbeans == 3)
			{
				Debug.LogFormat("[Rotten Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = 0;
				bool check = true;
				for (int i = 0; i < 9 && check; i++)
					if (beansafe[referArray[i]] && beansafe[i])
					{
						solution = referArray[i];
						check = false;
					}
				if (!striked)
				{
					if (!beansafe[referArray[pos]])
					{
						Debug.LogFormat("[Rotten Beans #{0}] You shouldn't eat smelly beans.", _moduleID);
						Module.HandleStrike();
						striked = true;
						StartCoroutine(Strike());
					}
					else if (!beansafe[pos])
					{
						Debug.LogFormat("[Rotten Beans #{0}] You shouldn't touch smelly beans.", _moduleID);
						Module.HandleStrike();
						striked = true;
						StartCoroutine(Strike());
					}
					else if (referArray[pos] != solution)
					{
						Debug.LogFormat("[Rotten Beans #{0}] Why did you eat bean {1} when bean {2} was perfectly available?", _moduleID, referArray[pos] + 1, solution + 1);
						Module.HandleStrike();
						striked = true;
						StartCoroutine(Strike());
					}
					else
						Debug.LogFormat("[Rotten Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, referArray[pos] + 1);
				}
				else
					Debug.LogFormat("[Beans #{0}] You might not know which beans were safe, the module will not strike you again.", _moduleID);
				beansafe[referArray[pos]] = false;
				eatenbeans++;
				if (eatenbeans == 3)
				{
					Module.HandlePass();
					Solve();
				}
			}
			Beans[referArray[pos]].transform.localScale = new Vector3(0f, 0f, 0f);
			referArray[pos] = referArray[referArray[pos]];
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		string[] colour = { " normal", " smelly" };
		if (eatenbeans == 0)
		{
			Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray[pos] % 3];
			Beans[referArray[pos]].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
		}
        else
			Text.GetComponent<TextMesh>().text = (pos + 1).ToString();
	}

	private void BeanHoverEnded(int pos)
	{
		Text.GetComponent<TextMesh>().text = "";
		if (eatenbeans == 0)
			Beans[referArray[pos]].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[referArray[pos]]] / 255f, colours[1][beanArray[referArray[pos]]] / 255f, colours[2][beanArray[referArray[pos]]] / 255f);
	}

	void Awake () {
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";

		for (int i = 0; i < Beans.Length; i++)
		{
			Beans[i].OnInteract += BeanPressed(i);
			int x = i;
			Beans[i].OnHighlight += delegate { BeanHovered(x); return; };
			Beans[i].OnHighlightEnded += delegate { BeanHoverEnded(x); return; };
		}

	}

	void Start () {

		bool[] valid = { true, false };

		bool ready = false;
		int[] solution = new int[3];
        for (int i = 0; i < 9; i++)
        {
			while(referArray[i] == -1 || referArray.Take(i).Contains(referArray[i]))
				referArray[i] = Rnd.Range(0, 9);
			offset[i] = Rnd.Range(0f, 360f);
		}
		Debug.LogFormat("[Rotten Beans #{0}] The chain is (position in list is start, value is end): {1}.", _moduleID, referArray.Select(x => x + 1).Join(", "));
		while (!ready)
		{
			bool[] validpos = { true, true, true, true, true, true, true, true, true };
			int eaten = 0;
			int[] backupRefer = referArray.ToArray();
            for (int i = 0; i < 9; i++)
				beanArray[i] = Rnd.Range(0, 2);
			for (int i = 0; i < 9; i++)
			{
				if (valid[beanArray[i]] && valid[beanArray[backupRefer[i]]] && eaten != 3 && validpos[i])
				{
					validpos[backupRefer[i]] = false;
					backupRefer[i] = backupRefer[backupRefer[i]];
					solution[eaten] = i;
					eaten++;
					i = -1;
				}
				if (eaten == 3)
					ready = true;
			}
		}
		Debug.LogFormat("[Rotten Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Select(x => "ns"[x].ToString()).Join(", "));
		Debug.LogFormat("[Rotten Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i]] / 255f, colours[1][beanArray[i]] / 255f, colours[2][beanArray[i]] / 255f);
			Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			beansafe[i] = valid[beanArray[i]];
		}
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(96 / 255f, 64 / 255f, 32 / 255f);
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(96 / 255f, 64 / 255f, 32 / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
	}

	private IEnumerator Cycle()
	{
		for (int i = 0; i < 9; i++)
		{
			while (i < 9 && Beans[i].transform.localScale.x < 0.01f)
				i++;
			if (i != 9)
			{
				Beans[i].OnHighlight();
				yield return new WaitForSeconds(0.9f);
				Beans[i].OnHighlightEnded();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat a bean in reading order (indexed at 1). No need to use spaces. Note that you cannot press an already eaten bean. e.g. '!{0} 139'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "cycle")
			StartCoroutine(Cycle());
		else
		{
			string validCommands = "123456789";
			command = command.Replace(" ", "");
			for (int i = 0; i < command.Length; i++)
				if (!validCommands.Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			yield return "strike";
			yield return "solve";
			for (int i = 0; eatenbeans != 3 && i < command.Length; i++)
			{
				yield return null;
				for (int j = 0; j < validCommands.Length; j++)
					if (command[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.01f) { Beans[j].OnInteract(); }
			}
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
			if (beansafe[referArray[i]] && beansafe[i])
			{
				Beans[i].OnInteract();
				i = -1;
				yield return null;
			}
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
            if (Beans[i].transform.localScale.x > 0.01f)
            {
				Beans[i].OnInteract();
				i = -1;
				yield return null;
			}
	}
}
