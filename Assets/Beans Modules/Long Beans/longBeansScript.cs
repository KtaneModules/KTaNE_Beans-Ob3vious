using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class longBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] BigBeans;
	public KMSelectable[] SmolBeans;
	public GameObject Text;
	public GameObject[] Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[10];
	private int eatenbeans = 0;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			if (eatenbeans == 3)
			{
				Debug.LogFormat("[Long Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int bean = 0;
				for (int i = 0; i < 10; i++)
					bean = bean * 2 + beanArray[i];
				if (pos != bean % 10)
				{
                    if (beanArray[bean % 10] == 0)
						Debug.LogFormat("[Long Beans #{0}] Bean {1} didn't exist, so you won't get a strike.", _moduleID, bean % 10);
                    else
                    {
						Debug.LogFormat("[Long Beans #{0}] Why did you eat bean {1}, when you were supposed to eat bean {2}?", _moduleID, pos, bean % 10);
						Module.HandleStrike();
					}
				}
				else
					Debug.LogFormat("[Long Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos);
				beanArray[pos] = 0;
				eatenbeans++;
				if (eatenbeans == 3)
				{
					Module.HandlePass();
					Solve();
				}
			}
			SmolBeans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private KMSelectable.OnInteractHandler BigBeanPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Chop", Module.transform);	
			BigBeans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		Text.GetComponent<TextMesh>().text = (pos).ToString();
	}

	private void BigBeanHovered(int pos)
	{
		Text.GetComponent<TextMesh>().text = (pos) + "/" + (pos + 5);
		BigBeans[pos].GetComponent<Renderer>().enabled = false;
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	private void BigBeanHoverEnded(int pos)
	{
		Text.GetComponent<TextMesh>().text = "";
		BigBeans[pos].GetComponent<Renderer>().enabled = true;
	}

	void Awake () {
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";

		for (int i = 0; i < SmolBeans.Length; i++)
		{
			SmolBeans[i].OnInteract += BeanPressed(i);
			int x = i;
			SmolBeans[i].OnHighlight += delegate { BeanHovered(x); return; };
			SmolBeans[i].OnHighlightEnded += delegate { BeanHoverEnded(); return; };
		}
		for (int i = 0; i < BigBeans.Length; i++)
		{
			BigBeans[i].OnInteract += BigBeanPressed(i);
			int x = i;
			BigBeans[i].OnHighlight += delegate { BigBeanHovered(x); return; };
			BigBeans[i].OnHighlightEnded += delegate { BigBeanHoverEnded(x); return; };
		}
	}

	void Start () {


		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			ready = true;
			beanArray = new int[10];
			for (int i = 0; i < 10; i++)
				beanArray[i] = Rnd.Range(0, 2);
			int[] beanspivot = beanArray.ToArray();
			for (int i = 0; i < 3; i++)
			{
				int bean = 0;
				for (int j = 0; j < 10; j++)
					bean = bean * 2 + beanspivot[j];
				if (beanspivot[bean % 10] == 0)
					ready = false;
				beanspivot[bean % 10] = 0;
				solution[i] = bean % 10;
			}
		}
		Debug.LogFormat("[Long Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Join(", "));
		Debug.LogFormat("[Long Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Join(", "));
		for (int i = 0; i < 10; i++)
			if (beanArray[i] == 0)
				SmolBeans[i].transform.localScale = new Vector3(0f, 0f, 0f);
	}

	private void Solve()
	{
		Statuslight[0].GetComponent<MeshRenderer>().enabled = false;
	}

	private IEnumerator CycleBig()
	{
		for (int i = 0; i < 5; i++)
		{
			while (i < 5 && BigBeans[i].transform.localScale.x < 0.01f)
				i++;
			if (i != 5)
			{
				BigBeans[i].OnHighlight();
				yield return new WaitForSeconds(0.9f);
				BigBeans[i].OnHighlightEnded();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	private IEnumerator CycleSmall()
	{
		for (int i = 0; i < 10; i++)
		{
			while (i < 10 && (SmolBeans[i].transform.localScale.x < 0.01f || BigBeans[i % 5].transform.localScale.x > 0.01f))
				i++;
			if (i != 10)
			{
				SmolBeans[i].OnHighlight();
				yield return new WaitForSeconds(0.9f);
				SmolBeans[i].OnHighlightEnded();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} cyclebig' to cycle through the big beans, '!{0} cyclesmall' to cycle through the small beans, '!{0} a' to cut the leftmost pea pod, '!{0} 0' to eat a small bean in reading order (indexed at 0). No need to use spaces. Note that you cannot press an already eaten bean. e.g. '!{0} 139'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "cyclebig")
			StartCoroutine(CycleBig());
		else if(command == "cyclesmall")
			StartCoroutine(CycleSmall());
		else
		{
			string validCommands = "0123456789abcde";
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
				{
					if (j < 10)
						if (command[i] == validCommands[j] && SmolBeans[j].transform.localScale.x > 0.01f)
						{
							if (BigBeans[j % 5].transform.localScale.x > 0.01f)
							{
								BigBeans[j % 5].OnInteract();
								yield return new WaitForSeconds(0.25f);
							}
							SmolBeans[j].OnInteract();
						}
					else if (command[i] == validCommands[j] && BigBeans[j - 10].transform.localScale.x > 0.01f)
							BigBeans[j - 10].OnInteract();
				}
				yield return new WaitForSeconds(0.5f);
			}
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
        for (int i = 0; i < 5; i++)
			if (BigBeans[i].transform.localScale.x > 0.01f)
			{
				BigBeans[i].OnInteract();
				yield return null;
			}
		for (int i = 0; i < 3 && eatenbeans != 3; i++)
		{
			int bean = 0;
			for (int j = 0; j < 10; j++)
				bean = bean * 2 + beanArray[j];
            if (SmolBeans[bean % 10].transform.localScale.x > 0.01f)
				SmolBeans[bean % 10].OnInteract();
            else
				for (int j = 0; j < 10; j++)
                    if (beanArray[j] == 1)
                    {
						SmolBeans[j].OnInteract();
						j = 10;
					}
			yield return null;
		}
	}
}
