using UnityEngine;
using System.Collections;


namespace LibSequentia.Data
{
	public partial class Layer
	{
		public static Layer GenTestLayer(IAudioClipHandle handle)
		{
			var layer	= new Layer();
			layer.clipHandle	= handle;

			return layer;
		}
	}

	public partial class Section
	{
		static Automation[] TestAutomation()
		{
			var tenauto1			= new Automation();
			tenauto1.targetParam	= Automation.TargetParam.Volume;
			tenauto1.AddPoint(0, 0);
			tenauto1.AddPoint(0.75f, 1);

			var tenauto2			= new Automation();
			tenauto2.targetParam	= Automation.TargetParam.Volume;
			tenauto2.AddPoint(0, 0);
			tenauto2.AddPoint(0.25f, 0);
			tenauto2.AddPoint(0.5f, 1);

			var tenauto3			= new Automation();
			tenauto3.targetParam	= Automation.TargetParam.Volume;
			tenauto3.AddPoint(0, 0);
			tenauto3.AddPoint(0.5f, 0);
			tenauto3.AddPoint(1f, 1);

			return new Automation[] { tenauto1, tenauto2, tenauto3 };
		}

		public static Section GenTestSection1(IAudioClipPack clipPack)
		{
			var section	= new Section();

			// BPM:120, 4/4 -> 1bar = 4beat

			var automations		= TestAutomation();

			var layer1			= Layer.GenTestLayer(clipPack.GetHandle("test-sec1-1"));
			section.m_layers[0]	= layer1;

			var layer2			= Layer.GenTestLayer(clipPack.GetHandle("test-sec1-2"));
			layer2.AddTensionAutomation(automations[1]);
			section.m_layers[1]	= layer2;

			var layer3			= Layer.GenTestLayer(clipPack.GetHandle("test-sec1-3"));
			layer3.AddTensionAutomation(automations[0]);
			section.m_layers[2]	= layer3;

			var layer4			= Layer.GenTestLayer(clipPack.GetHandle("test-sec1-4"));
			layer4.AddTensionAutomation(automations[2]);
			section.m_layers[3]	= layer4;

			section.inTypeNatural	= InType.KickIn;
			section.inTypeManual	= InType.FadeIn;
			section.outTypeNatural	= OutType.LeaveIt;
			section.outTypeManual	= OutType.FadeOut;
			section.doNotOverlapFillIn	= false;

			section.beatFillIn		= 0;
			section.beatStart		= 1 * 4;
			section.beatEnd			= 9 * 4;

			return section;
		}

		public static Section GenTestSection2(IAudioClipPack clipPack)
		{
			var section	= new Section();

			// BPM:120, 4/4 -> 1bar = 4beat

			var automations		= TestAutomation();

			var layer1			= Layer.GenTestLayer(clipPack.GetHandle("test-sec2-1"));
			section.m_layers[0]	= layer1;

			var layer2			= Layer.GenTestLayer(clipPack.GetHandle("test-sec2-2"));
			layer2.AddTensionAutomation(automations[1]);
			section.m_layers[1]	= layer2;

			var layer3			= Layer.GenTestLayer(clipPack.GetHandle("test-sec2-3"));
			layer3.AddTensionAutomation(automations[0]);
			section.m_layers[2]	= layer3;

			var layer4			= Layer.GenTestLayer(clipPack.GetHandle("test-sec2-4"));
			layer4.AddTensionAutomation(automations[2]);
			section.m_layers[3]	= layer4;

			section.inTypeNatural	= InType.KickIn;
			section.inTypeManual	= InType.FadeIn;
			section.outTypeNatural	= OutType.LeaveIt;
			section.outTypeManual	= OutType.FadeOut;
			section.doNotOverlapFillIn	= false;

			section.beatFillIn		= 0;
			section.beatStart		= 1 * 4;
			section.beatEnd			= 9 * 4;

			return section;
		}

		public static Section GenTestSection3(IAudioClipPack clipPack)
		{
			var section	= new Section();

			// BPM:120, 4/4 -> 1bar = 4beat

			var automations		= TestAutomation();

			var layer1			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec1-1"));
			section.m_layers[0]	= layer1;

			var layer2			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec1-2"));
			layer2.AddTensionAutomation(automations[1]);
			section.m_layers[1]	= layer2;

			var layer3			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec1-3"));
			layer3.AddTensionAutomation(automations[0]);
			section.m_layers[2]	= layer3;

			var layer4			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec1-4"));
			layer4.AddTensionAutomation(automations[2]);
			section.m_layers[3]	= layer4;

			section.inTypeNatural	= InType.KickIn;
			section.inTypeManual	= InType.FadeIn;
			section.outTypeNatural	= OutType.LeaveIt;
			section.outTypeManual	= OutType.FadeOut;
			section.doNotOverlapFillIn	= false;

			section.beatFillIn		= 0;
			section.beatStart		= 1 * 2;
			section.beatEnd			= section.beatStart + 8 * 4;

			return section;
		}

		public static Section GenTestSection4(IAudioClipPack clipPack)
		{
			var section	= new Section();

			// BPM:120, 4/4 -> 1bar = 4beat

			var automations		= TestAutomation();

			var layer1			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec2-1"));
			section.m_layers[0]	= layer1;

			var layer2			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec2-2"));
			layer2.AddTensionAutomation(automations[1]);
			section.m_layers[1]	= layer2;

			var layer3			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec2-3"));
			layer3.AddTensionAutomation(automations[0]);
			section.m_layers[2]	= layer3;

			var layer4			= Layer.GenTestLayer(clipPack.GetHandle("test2-sec2-4"));
			layer4.AddTensionAutomation(automations[2]);
			section.m_layers[3]	= layer4;

			section.inTypeNatural	= InType.KickIn;
			section.inTypeManual	= InType.FadeIn;
			section.outTypeNatural	= OutType.LeaveIt;
			section.outTypeManual	= OutType.FadeOut;
			section.doNotOverlapFillIn	= false;

			section.beatFillIn		= 0;
			section.beatStart		= 1 * 1;
			section.beatEnd			= section.beatStart + 8 * 4;

			return section;
		}
	}

	public partial class Track
	{
		public static Track GenTestTrack(float bpm, IAudioClipPack clipPack)
		{
			var track			= new Track();

			track.BPM			= bpm;

			var section1		= Section.GenTestSection1(clipPack);
			var section2		= Section.GenTestSection2(clipPack);

			track.m_sectionSeq.Add(section1);
			track.m_sectionSeq.Add(section2);
			track.m_sectionSeq.Add(section1);
			
			return track;
		}

		public static Track GenTestTrack2(float bpm, IAudioClipPack clipPack)
		{
			var track			= new Track();

			track.BPM			= bpm;

			var section1		= Section.GenTestSection3(clipPack);
			var section2		= Section.GenTestSection4(clipPack);

			track.m_sectionSeq.Add(section1);
			track.m_sectionSeq.Add(section2);
			track.m_sectionSeq.Add(section1);
			
			return track;
		}
	}

	public partial class TransitionScenario
	{
		public static TransitionScenario GenTestScenario()
		{
			var scen				= new TransitionScenario();

			var intro_vol			= new Automation();
			intro_vol.targetParam	= Automation.TargetParam.Volume;
			intro_vol.AddPoint(0, 0);
			intro_vol.AddPoint(1, 1);
			scen.AddIntroAutomation(intro_vol);

			var outro_vol			= new Automation();
			outro_vol.targetParam	= Automation.TargetParam.Volume;
			outro_vol.AddPoint(1, 0);
			outro_vol.AddPoint(0, 1);
			scen.AddOutroAutomation(outro_vol);

			return scen;
		}

		public static TransitionScenario GenTestScenario2()
		{
			var scen				= new TransitionScenario();

			var intro_vol			= new Automation();
			intro_vol.targetParam	= Automation.TargetParam.Volume;
			intro_vol.AddPoint(0, 0);
			intro_vol.AddPoint(0.5f, 1);
			intro_vol.AddPoint(1.0f, 1);
			scen.AddIntroAutomation(intro_vol);
			var intro_lowcut		= new Automation();
			intro_lowcut.targetParam	= Automation.TargetParam.LowCut;
			intro_lowcut.AddPoint(0, 1);
			intro_lowcut.AddPoint(0.5f, 1);
			intro_lowcut.AddPoint(0.5f, 0);
			intro_lowcut.AddPoint(1, 0);
			scen.AddIntroAutomation(intro_lowcut);

			var outro_vol			= new Automation();
			outro_vol.targetParam	= Automation.TargetParam.Volume;
			outro_vol.AddPoint(0, 1);
			outro_vol.AddPoint(0.5f, 1);
			outro_vol.AddPoint(1, 0);
			scen.AddOutroAutomation(outro_vol);
			var outro_lowcut		= new Automation();
			outro_lowcut.targetParam	= Automation.TargetParam.LowCut;
			outro_lowcut.AddPoint(0, 0);
			outro_lowcut.AddPoint(0.5f, 0);
			outro_lowcut.AddPoint(0.5f, 1);
			outro_lowcut.AddPoint(1, 1);
			scen.AddOutroAutomation(outro_lowcut);

			return scen;
		}
	}
}