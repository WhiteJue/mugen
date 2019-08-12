/* MUGEN
实时内存参数读取 Real-Time DebugArgs Reader
Date:2019-8-12
Author:白绝 https://kfm.ink/
ReadMe：
该源码在CSC.EXE编译方式下编写
该源码推荐的编译参数 csc.exe memory.cs
*/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace memory
{
	class read
	{
	//打开进程，取得操作句柄
	[DllImport("Kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
    	public static extern int OpenProcess(int com, int hwnd, int pid);

    	//关闭操作句柄
     	[DllImport("Kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    	public static extern bool CloseHandle(int hP);

    	//读取进程内存 整数型
		[DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
    	public static extern bool ReadProcessMemory(int hP, int addr, out int ret, int len, int writeLen);

    	//读取进程内存 小树型
		[DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
    	public static extern bool ReadProcessMemory(int hP, int addr, out float ret, int len, int writeLen);    	

    	//读取进程内存 文本型
    	//数组默认为引用类型 无需使用关键字out
    	[DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
    	public static extern bool ReadProcessMemory(int hP, int addr, byte[] ret, int len, out int writeLen);


    	private int pid;
    	private int hP;

    	//构造器
    	public read(int apid)
    	{
    		pid = apid;
    		hP = OpenProcess(2035711, 0, pid);// #PROCESS_ALL_ACCESS 2035711
    	
    	}

    	public int readInt32(int addr)
    	{
    		int aint;
    		bool ret;
    		ret = ReadProcessMemory(hP, addr, out aint, 4, 0);
    		return ret ? aint : 0;
    	}

    	public float readFloat(int addr)
    	{
    		float afloat;
    		bool ret;
    		ret = ReadProcessMemory(hP, addr, out afloat, 4, 0);
    		return ret ? afloat : 0;
    	}

    	public string readString(int addr, int len)
    	{
    		byte[] data = new byte[len];
    		bool ret = ReadProcessMemory(hP, addr, data, len, out len);
    		if(ret == false)
    		{
    			return "";
    		}
    		int leftlen = Array.IndexOf(data, (byte)0) + 1;
    		if(leftlen == 0)
    		{
    			return System.Text.Encoding.ASCII.GetString(data);
    		}
    		byte[] left = new byte[leftlen];
    		Array.Copy(data, left, leftlen);
    		return System.Text.Encoding.ASCII.GetString(left);
    	}

	}

	class mugenArgs
	{
		private read mr;
		private int main;//基址
		private bool is10;
		public mugenArgs(int pid, bool ais10)
		{
			mr = new read(pid);
			is10 = ais10;
			if(is10)
			{
				//1.0主程序基址获取 务必使用循环读取 否则可能导致读取不到正确的值
				do
				{
					System.Threading.Thread.Sleep(500); //此处可能造成事件独占 导致假死 可优化
					main = mr.readInt32(5418692);
					if(main > 0)
					{
						main += 15332;
					}

				}while(main == 0);

			}
			else
			{
				//win主程序基址获取
				main = mr.readInt32(4938572);//4B5B4C
			}
		}

		//获取人物地址 偏移为4 以此类推 前四个为Player1-4 之后为子Helper或Player
		public int p1Addr() => mr.readInt32(main + 46932);

		public int p2Addr() => mr.readInt32(main + 46932 + 4);

		public int p3Addr() => mr.readInt32(main + 46932 + 8);

		public int p4Addr() => mr.readInt32(main + 46932 + 12);

		public int playerAddr(int index) => mr.readInt32(main + 46932 + index * 4);

		//获取人物信息数据地址
		public int playerInfoAddr(int playerAddr) => mr.readInt32(playerAddr);

		//以下为人物参数
		public int alive(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3712) : mr.readInt32(playerAddr + 3620);

			//获取人物活动标志 当此值为0时表示 is-frozen = true
		public int activeFlag(int playerNo) => mr.readInt32(main + 47720 + playerNo * 4);

		public int life(int playerAddr) => is10 ? mr.readInt32(playerAddr + 440) : mr.readInt32(playerAddr + 352);

		public int power(int playerAddr) => is10 ? mr.readInt32(playerAddr + 472) : mr.readInt32(playerAddr + 376);

		public int palno(int playerAddr) => (is10 ? mr.readInt32(playerAddr + 5152) : mr.readInt32(playerAddr + 5060)) + 1;

		public int attackMulSet(int playerAddr) => is10 ? mr.readInt32(playerAddr + 480) : mr.readInt32(playerAddr + 392);

		public int ctrl(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3684) : mr.readInt32(playerAddr + 3596);

		public int damage(int playerAddr) => is10 ? mr.readInt32(playerAddr + 4228) : mr.readInt32(playerAddr + 4136);

		public string displayerName(int playerAddr) => mr.readString(playerInfoAddr(playerAddr) + 48, 256);

		public bool doesExist(int playerAddr) => (is10 ? mr.readInt32(playerAddr + 432) : mr.readInt32(playerAddr + 344)) != 0;

		public int facing(int playerAddr) => is10 ? mr.readInt32(playerAddr + 488) : mr.readInt32(playerAddr + 400);

		public int fallDamage(int playerAddr) => is10 ? mr.readInt32(playerAddr + 4300) : mr.readInt32(playerAddr + 4212);

		public int playerId(int playerAddr) => mr.readInt32(playerAddr + 4);

		public int teamSide(int playerAddr) => mr.readInt32(playerAddr + 12);

		public float velX(int playerAddr) => is10 ? mr.readFloat(playerAddr + 516) : mr.readFloat(playerAddr + 436);

		public float velY(int playerAddr) => is10 ? mr.readFloat(playerAddr + 520) : mr.readFloat(playerAddr + 440);

		public int playerNoFromId(int id)
		{
			int pAddr;
			for(int i = 0; i < 60; i++)
			{
				pAddr = playerAddr(i);
				if(pAddr != 0 && doesExist(pAddr) && id == playerId(pAddr))
				{
					return i;
				}
			}
			return -1;
		}

		public float posX(int playerAddr) => is10 ? (mr.readFloat(playerAddr + 500) - screenX() / 2) : (mr.readFloat(playerAddr + 412) - screenX() - 160);

		public float posY(int playerAddr) => is10 ? (mr.readFloat(playerAddr + 504) / 2) : mr.readFloat(playerAddr + 416);

		public int prevstateNo(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3152) : mr.readInt32(playerAddr + 3064);

		public int stateNo(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3148) : mr.readInt32(playerAddr + 3060);

			//内部方法 请调用statetypestr
		private int stateType(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3672) : mr.readInt32(playerAddr + 3584);

		public string stateTypestr(int playerAddr)
		{
			int astateType = stateType(playerAddr);
			if(astateType < 1 || astateType > 4)
			{
				return "";
			}
			switch(astateType)
			{
				case 1:
				return "S";

				case 2:
				return "C";
				
				case 3:
				return "A";

				case 4:
				return "L";

				default:
				return "";
			}

		}


		public int rootId(int id)
		{
			int playerNo = playerNoFromId(id);
			int pAddr = 0;
			if(playerNo == -1)
			{
				return -1;
			}
			if(playerNo < 4)
			{
				return id;
			}
			while(playerNo > 3)
			{
				pAddr = playerAddr(playerNo);
				playerNo = playerNoFromId(parentId(pAddr));
			}
			return parentId(pAddr);

		}

		public int movecontact(int playerAddr)
		{
			int flag = is10 ? mr.readInt32(playerAddr + 3724) : mr.readInt32(playerAddr + 3632);
			if(flag == 1 || flag == 2)
			{
				return is10 ? mr.readInt32(playerAddr + 3728) : mr.readInt32(playerAddr + 3636);
			}
			return 0;
		}

		public int moveguarded(int playerAddr)
		{
			int flag = is10 ? mr.readInt32(playerAddr + 3724) : mr.readInt32(playerAddr + 3632);
			if(flag == 1)
			{
				return is10 ? mr.readInt32(playerAddr + 3728) : mr.readInt32(playerAddr + 3636);
			}
			return 0;
		}

		public int movehit(int playerAddr)
		{
			int flag = is10 ? mr.readInt32(playerAddr + 3724) : mr.readInt32(playerAddr + 3632);
			if(flag == 2)
			{
				return is10 ? mr.readInt32(playerAddr + 3728) : mr.readInt32(playerAddr + 3636);
			}
			return 0;
		}

		public int movereversed(int playerAddr)
		{
			int flag = is10 ? mr.readInt32(playerAddr + 3724) : mr.readInt32(playerAddr + 3632);
			if(flag == 4)
			{
				return is10 ? mr.readInt32(playerAddr + 3728) : mr.readInt32(playerAddr + 3636);
			}
			return 0;
		}

			//内部方法 请调用movetypestr
		private int moveType(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3676) : mr.readInt32(playerAddr + 3588);

		public string moveTypestr(int playerAddr)
		{
			int movetype = moveType(playerAddr);
			if(movetype < 0 || movetype > 2)
			{
				return "";
			}
			switch(movetype)
			{
				case 0:
				return "I";
				case 1:
				return "A";
				case 2:
				return "H";
				default:
				return "";
			}
		}

			//使用attrstr获取文本形式的参数
		public int hitoverride(int playerAddr, int index)
		{
			int addr = is10 ? (playerAddr + 4356 + 20 * index) : (playerAddr + 4264 + 20 * index);
			if(mr.readInt32(addr) == 0)
			{
				return 0;
			}
			return mr.readInt32(addr + 4);
		}

		public int hitpausetime(int playerAddr) => is10 ? mr.readInt32(playerAddr + 3696) : mr.readInt32(playerAddr + 3508);

		public int pauseMoveTime(int playerAddr) => is10 ? mr.readInt32(playerAddr + 552) : mr.readInt32(playerAddr + 476);

		public int superpausemovetime(int playerAddr) => is10 ? mr.readInt32(playerAddr + 556) : mr.readInt32(playerAddr + 480);

			//仅适用于Helper
		public int helperId(int playerAddr) => is10 ? mr.readInt32(playerAddr + 5236) : mr.readInt32(playerAddr + 9752);

			//仅适用于Helper
		public int parentId(int playerAddr) => is10 ? mr.readInt32(playerAddr + 5240) : mr.readInt32(playerAddr + 9756);

			//内部方法 请调用hitbystr
		private int hitby(int playerAddr) => is10 ? (mr.readInt32(playerAddr + 4184) & mr.readInt32(playerAddr + 4188)) : (mr.readInt32(playerAddr + 4092) & mr.readInt32(playerAddr + 4096));

			//内部方法
		private int noclsn2flag(int playerAddr)
		{
			int addr1 = is10 ? mr.readInt32(playerAddr + 5144) : mr.readInt32(playerAddr + 5052);
			if(addr1 == 0)
			{
				return 1;
			}
			int addr2 = mr.readInt32(addr1 + 16);
			return (addr2 == 0 || mr.readInt32(addr2 + 44) == 0) ? 1 : 0;

		}

			//内部方法
		private int muteki(int playerAddr) => is10 ? mr.readInt32(playerAddr + 4180) : mr.readInt32(playerAddr + 4088);

		public string attrstr(int hitby)
		{
			string str = "";
			if((hitby & 1) != 0)
			{
				str += "s";
			}

			if((hitby & 2) != 0)
			{
				str += "c";
			}

			if((hitby & 3) != 0)
			{
				str += "a";
			}

			if((hitby & 56) == 56)
			{
				str += ",aa";
			}
			else
			{
				if((hitby & 8) != 0)
				{
					str += ",na";
				}

				if((hitby & 16) != 0)
				{
					str += ",sa";
				}

				if((hitby & 32) != 0)
				{
					str += ",ha";
				}
			}

			if((hitby & 448) == 448)
			{

				str += ",at";
			}
			else
			{
				if((hitby & 64) != 0)
				{
					str += ",nt";
				}

				if((hitby & 128) != 0)
				{
					str += ",st";
				}

				if((hitby & 256) != 0)
				{
					str += ",ht";
				}

			}

			if((hitby & 3584) == 3584)
			{
				str += ",ap";
			}
			else
			{
				if((hitby & 512) != 0)
				{
					str += ",np";
				}

				if((hitby & 1024) != 0)
				{
					str += ",sp";
				}

				if((hitby & 2048) != 0)
				{
					str += ",hp";
				}

			}
			return str;
		}

		public string hitbystr(int playerAddr)
		{
			int ahitby = hitby(playerAddr);
			if(noclsn2flag(playerAddr) == 0)
			{
				if(muteki(playerAddr) == 0)
				{
					if((ahitby & 7) != 0)
					{
						if((ahitby & 228) != 0)
						{
							return attrstr(ahitby);
						}
						else
						{
							return "muteki";
						}

					}
					else
					{
						return "muteki";
					}

				}
				else
				{
					return "unhittable";
				}

			}
			else
			{
				return "no-clsn2";
			}

		}

		public int target(int playerAddr, int no)
		{
			int addr1 = is10 ? mr.readInt32(playerAddr + 620) : mr.readInt32(playerAddr + 544);
			if(addr1 == 0 || mr.readInt32(addr1 + 8) <= no)
			{
				return 0;
			}
			int addr2 = is10 ? mr.readInt32(addr1 + 24) : mr.readInt32(addr1 + 20);
			if(addr2 == 0)
			{
				return 0;
			}
			int paddr = is10 ? mr.readInt32(addr2 + 28 * no) : mr.readInt32(addr2 + 32 * no);
			if(paddr == 0)
			{
				return 0;
			}
			return playerId(paddr);

		}

		//系统参数
		public int intro() => ((mr.readInt32(main + 47992) & 1) == 0) ? 0 : 1;

		public bool isPaused() => (is10 ? mr.readInt32(5421392) : mr.readInt32(4942465)) != 0;

		public int noko() => ((mr.readInt32(main + 47992) & 65536) == 0) ? 0 : 1;

		public int pauseTime() => mr.readInt32(main + 48084);

		public int superPauseTime() => mr.readInt32(main + 48120);

		public int roundNoOfMatch() => is10 ? mr.readInt32(main + 2584) : mr.readInt32(main + 23544);

		public int roundNoToVer() => ((mr.readInt32(main + 47992) & 256) == 0) ? 0 : 1;

		public int timeFreeze() => ((mr.readInt32(main + 47992 + 4) & 16777216) == 0) ? 0 : 1;

		public int roundState() => mr.readInt32(main + 48176);

		public int roundNo() => mr.readInt32(main + 48132);

		public int roundtime() => mr.readInt32(main + 48192);

		public float screenX() => is10 ? mr.readFloat(main + 46112) : mr.readFloat(main + 46120);

		public float screenY() => is10 ? mr.readFloat(main + 46116) : mr.readFloat(main + 46132);

		public bool team1Win() => mr.readInt32(main + 48180) == 1;

		public bool team2Win() => mr.readInt32(main + 48180) == 2;

		//变量
		public int[] sysVars(int playerAddr)
		{
			List<int> lst = new List<int>();
			int offset = is10 ? 4136 : 4044;
			for(int i = 1; i < 6; i++)
			{
				lst.Add(mr.readInt32(playerAddr + offset + 4 * i));
			}
			return lst.ToArray();
		}

		public float[] sysFvars(int playerAddr)
		{
			List<float> lst = new List<float>();
			int offset = is10 ? 4156 : 4064;
			for(int i = 1; i < 6; i++)
			{
				lst.Add(mr.readFloat(playerAddr + offset + 4 * i));
			}
			return lst.ToArray();
		} 

		public int[] vars(int playerAddr)
		{
			List<int> lst = new List<int>();
			int offset = is10 ? 3736 : 3644;
			for(int i = 1; i < 61; i++)
			{
				lst.Add(mr.readInt32(playerAddr + offset + 4 * i));
			}
			return lst.ToArray();

		}

		public float[] fvars(int playerAddr)
		{
			List<float> lst = new List<float>();
			int offset = is10 ? 3976 : 3884;
			for(int i = 1; i < 41; i++)
			{
				lst.Add(mr.readFloat(playerAddr + offset + 4 * i));
			}
			return lst.ToArray();

		}

		//explod
		public int explodListAddr() => is10 ? mr.readInt32(main + 41424) : mr.readInt32(main + 41700);

		public int explodAddr(int explodListAddr, int index) => is10 ? mr.readInt32(explodListAddr + 264 * index) : mr.readInt32(explodListAddr + 228 * index);

		public bool explodExist(int explodAddr) => mr.readInt32(explodAddr) != 0;

		public int explodId(int explodAddr) => mr.readInt32(explodAddr + 16);

		public int explodOwnerId(int explodAddr) => mr.readInt32(explodAddr + 12);

		public int animIndex(int explodAddr)
		{
			int addr = mr.readInt32(explodAddr + 128);
			return (addr == 0) ? -1 : mr.readInt32(addr + 12);
		}

		//proj
		public int projBaseAddr(int playerAddr) => is10 ? mr.readInt32(playerAddr + 616) : mr.readInt32(playerAddr + 540);

		public int projListAddr(int projBaseAddr) => is10 ? mr.readInt32(projBaseAddr + 24) : mr.readInt32(projBaseAddr + 20);		

		public int projAddr(int projListAddr, int index) => is10 ? mr.readInt32(projListAddr + index * 736) : mr.readInt32(projListAddr + index * 732);

		public bool projExist(int projBaseAddr, int projNo, int projAddr) => projNo <= mr.readInt32(projBaseAddr + 40) && mr.readInt32(projAddr + 4) == 1;

		public int projId(int projAddr) => mr.readInt32(projAddr);

		public float projX(int projAddr) => is10 ? (mr.readInt32(projAddr + 68) + 320) : mr.readInt32(projAddr + 92); 

		public float projY(int projAddr) => is10 ? (mr.readInt32(projAddr + 72) + 240) : mr.readInt32(projAddr + 96);	

		//anim
		public int animListAddr(int playerAddr)
		{
			int pInfoAddr = playerInfoAddr(playerAddr);
			if(pInfoAddr == 0)
			{
				return 0;
			}
			int addr1 = is10 ? mr.readInt32(pInfoAddr + 1080) : mr.readInt32(pInfoAddr + 972);
			if(addr1 == 0)
			{
				return 0;
			}
			int addr2 = mr.readInt32(addr1);
			if(addr2 == 0)
			{
				return 0;
			}
			int targetAddr = is10 ? mr.readInt32(addr2 + 28) : mr.readInt32(addr2 + 24);
			if(checkAnimList(targetAddr))
			{
				return targetAddr;
			}
			else
			{
				return 0;
			}
		}

		private bool checkAnimList(int targetAddr)
		{
			if(mr.readInt32(targetAddr) != 1)
			{
				return false;
			}
			if(mr.readInt32(targetAddr + 4) != 0)
			{
				return false;
			}
			if(mr.readInt32(targetAddr + 16) != 1)
			{
				return false;
			}
			if(mr.readInt32(targetAddr + 20) != 1)
			{
				return false;
			}
			if(mr.readInt32(targetAddr + 32) != 1)
			{
				return false;
			}
			if(mr.readInt32(targetAddr + 36) != 2)
			{
				return false;
			}
			return true;
		}

		public int animNo(int animListAddr, int index) => mr.readInt32(animListAddr + index * 16 + 12);
	}
	class test
	{
		//此处仅供测试
		static void Main(string[] args)
		{
			mugenArgs m = new mugenArgs(8100, false);
			while(true)
			{
				Console.WriteLine(m.stateNo(m.p1Addr()));
				System.Threading.Thread.Sleep(500);
			}	
		}
	}
}
