using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class coolBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;
	public Material meltmat;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[] timeoffset = new int[9];
	private int[][] colours = new int[][] { new int[] { 255, 255, 0 }, new int[] { 112, 192, 255 }, new int[] { 0, 0, 0 } };
	private bool[] beansafe = new bool[9];
	private int eatenbeans = 0;
	private bool[] safetypes = { true, true, true };
	private bool[] frozen = { true, true, true, true, true, true, true, true, true };
	private bool heating = false;

	private KMAudio.KMAudioRef sound;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed(int pos)
	{
		return delegate
		{
			if (frozen[pos])
			{
				sound = Audio.PlaySoundAtTransformWithRef("Mmmmmicrowave", Module.transform);
				heating = true;
				StartCoroutine(BeanHeating(pos));
			}
			else
			{
				Audio.PlaySoundAtTransform("Monch", Module.transform);
				if (eatenbeans == 3)
				{
					Debug.LogFormat("[Cool Beans #{0}] They know...", _moduleID);
					Module.HandleStrike();
				}
				else
				{
					int solution = -1;
					bool check = true;
					for (int i = 8; i > -1 && check; i--)
						if (beansafe[i] && safetypes[beanArray[i]])
						{
							solution = i;
							check = false;
						}
					if (solution != -1)
					{
						if (!beansafe[pos])
						{
							string[] colour = { "n orange", " yellow", " green" };
							if (safetypes[beanArray[pos]])
								Debug.LogFormat("[Cool Beans #{0}] Eating bean {1} became unhygienic, remember?", _moduleID, pos + 1);
							else
								Debug.LogFormat("[Cool Beans #{0}] You already ate a{1} bean.", _moduleID, colour[beanArray[pos]]);
							Module.HandleStrike();
							StartCoroutine(Strike());
						}
						else if (pos != solution)
						{
							Debug.LogFormat("[Cool Beans #{0}] Why did you eat bean {1} when bean {2} was perfectly available?", _moduleID, pos + 1, solution + 1);
							Module.HandleStrike();
							StartCoroutine(Strike());
						}
						else
							Debug.LogFormat("[Cool Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos + 1);
					}
					else
						Debug.LogFormat("[Cool Beans #{0}] There was no valid bean, so you will not get a strike.", _moduleID);
					beansafe[pos] = false;
					if (pos / 3 != 0)
						beansafe[pos - 3] = false;
					if (pos / 3 != 2)
						beansafe[pos + 3] = false;
					if (pos % 3 != 0)
						beansafe[pos - 1] = false;
					if (pos % 3 != 2)
						beansafe[pos + 1] = false;
					safetypes[beanArray[pos]] = false;
					eatenbeans++;
					if (eatenbeans == 3)
					{
						Module.HandlePass();
						Solve();
					}
				}
				Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			}
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		if (!frozen[pos])
		{
			string[] colour = { " orange", " yellow", " green" };
			Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray[pos]];
		}
        else
			Text.GetComponent<TextMesh>().text = (pos + 1).ToString();
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	void Awake() {
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";

		for (int i = 0; i < Beans.Length; i++)
		{
			Beans[i].OnInteract += BeanPressed(i);
			Beans[i].OnInteractEnded += delegate { heating = false; if (sound != null) { sound.StopSound(); sound = null; } return; };
			int x = i;
			Beans[i].OnHighlight += delegate { BeanHovered(x); return; };
			Beans[i].OnHighlightEnded += delegate { BeanHoverEnded(); return; };
		}
	}

	void Start() {

		string chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0";
		int[] logvalid = { 0, 0 };

		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 36; j++)
				if ((BombInfo.GetSerialNumber()[i] == chars[j]))
					logvalid[i] = j % 9;
		Debug.LogFormat("[Cool Beans #{0}] Unreheatable beans are: {1}.", _moduleID, logvalid.Select(x => x + 1).Join(", "));

		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			bool[] validpos = { true, true, true, true, true, true, true, true, true };
			for (int i = 0; i < 2; i++)
				for (int j = 0; j < 36; j++)
					if ((BombInfo.GetSerialNumber()[i] == chars[j]))
						validpos[j % 9] = false;
			int eaten = 0;
			bool[] valid = { true, true, true };
			for (int i = 8; i > -1; i--)
			{
				timeoffset[i] = Rnd.Range(0, 100);
				beanArray[i] = Rnd.Range(0, 3);
				if (valid[beanArray[i]] && validpos[i] && eaten != 3)
				{
					if (i / 3 != 0)
						validpos[i - 3] = false;
					if (i / 3 != 2)
						validpos[i + 3] = false;
					if (i % 3 != 0)
						validpos[i - 1] = false;
					if (i % 3 != 2)
						validpos[i + 1] = false;
					solution[eaten] = i;
					valid[beanArray[i]] = false;
					eaten++;
				}
				if (eaten == 3)
					ready = true;
			}
		}
		Debug.LogFormat("[Cool Beans #{0}] The beans are: {1}.", _moduleID, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Select(x => logvalid.Contains(x) ? "x" : "oyg"[beanArray[x]].ToString()).Join(", "));
		Debug.LogFormat("[Cool Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			beansafe[i] = true;
		}
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 36; j++)
				if ((BombInfo.GetSerialNumber()[i] == chars[j]))
					beansafe[j % 9] = false;
		Statuslight.GetComponent<MeshRenderer>().material = meltmat;
		StartCoroutine(Wobble());
	}

	private IEnumerator Wobble()
	{
		float t = 0;
		while (true)
		{
			for (int i = 0; i < 9; i++)
				if (frozen[i])
					Beans[i].transform.localEulerAngles = new Vector3(0f, Mathf.Sin(t + timeoffset[i]) * 5f + offset[i], 0f);
				else
					Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			t += 3f;
			yield return new WaitForSeconds(0.02f);
		}
	}

	private IEnumerator BeanHeating(int pos)
	{
		string chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0";
		int[] logvalid = { 0, 0 };
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 36; j++)
				if ((BombInfo.GetSerialNumber()[i] == chars[j]))
					logvalid[i] = j % 9;
		for (float t = 0; t < 2 && heating; t += Time.deltaTime)
			yield return null;
		if (heating)
		{
			if (logvalid.Contains(pos))
			{
				if (sound != null) { sound.StopSound(); sound = null; }
				Debug.LogFormat("[Cool Beans #{0}] That bean can't be heated.", _moduleID);
				Module.HandleStrike();
				StartCoroutine(Strike());
				heating = false;
			}
			else
			{
				frozen[pos] = false;
				Beans[pos].GetComponent<MeshRenderer>().material = meltmat;
				Beans[pos].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[pos] % 3] / 255f, colours[1][beanArray[pos] % 3] / 255f, colours[2][beanArray[pos] % 3] / 255f);
				for (float t = 0; t < 1 && heating; t += Time.deltaTime)
					yield return null;
				if (sound != null) { sound.StopSound(); sound = null; }
				if (heating)
				{
					Debug.LogFormat("[Cool Beans #{0}] You let the microwave go off.", _moduleID);
					Module.HandleStrike();
					StartCoroutine(Strike());
					Audio.PlaySoundAtTransform("Mbeep", Module.transform);
				}
			}
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
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
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat/reheat a bean in reading order (it will heat if it's frozen and eat otherwise). No need to use spaces. Note that you cannot press an already eaten bean. e.g. '!{0} 139'";
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
					if (command[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.01f)
					{
						Beans[j].OnInteract();
						yield return null;
						while (frozen[j] && heating)
							yield return null;
						Beans[j].OnInteractEnded();
					}
			}
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		string chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0";
		int[] logvalid = { 0, 0 };

		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 36; j++)
				if ((BombInfo.GetSerialNumber()[i] == chars[j]))
					logvalid[i] = j % 9;
		for (int i = 8; i > -1 && eatenbeans != 3; i--)
			if (beansafe[i] && safetypes[beanArray[i]])
			{
                if (frozen[i])
                {
					Beans[i].OnInteract();
					while (frozen[i] && heating)
						yield return null;
					Beans[i].OnInteractEnded();
					yield return null;
				}
				Beans[i].OnInteract();
				yield return null;
				Beans[i].OnInteractEnded();
				yield return null;
			}
		for (int i = 8; i > -1 && eatenbeans != 3; i--)
			if (Beans[i].transform.localScale.x > 0.01f)
			{
				Beans[i].OnInteract();
				yield return null;
			}
	}
}
