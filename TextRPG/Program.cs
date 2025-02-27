﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace TextRPG
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("스파르타 마을에 오신 것을 환영합니다.");
            Console.Write("당신의 이름을 입력하세요: ");
            string playerName = Console.ReadLine();

            Console.Write("직업을 선택하세요 (1: 전사 / 2: 마법사): ");
            string playerJob = "";

            while (true)
            {
                string jobChoice = Console.ReadLine();

                if (jobChoice == "1")
                {
                    playerJob = "전사";
                    break;      
                }    
                else if (jobChoice == "2")
                {
                    playerJob = "마법사";
                    break;
                }
                else
                {
                    Console.Write("잘못된 입력입니다. 다시 선택하세요 (1: 전사 / 2: 마법사):");
                }    
            }

            Player player = new Player { Name = playerName, Job = playerJob };
            Shop shop = new Shop();

            while (true)
            {
                Console.WriteLine("\n1. 상태 보기 \n2. 인벤토리 \n3. 상점 \n4. 휴식 \n5. 종료");
                Console.Write("원하는 행동을 입력하세요: ");
                string choice = Console.ReadLine();

                if (choice == "1") player.ShowStatus();
                else if (choice == "2") player.ShowInventory();
                else if (choice == "3") shop.OpenShop(player);
                else if (choice == "4") player.Rest();
                else if (choice == "5") 
                {
                    Console.WriteLine("게임을 종료합니다.");
                    break;
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                }
            }
        }
        class Player
        {
            public string Name { get; set; }
            public string Job { get; set; }
            public int Level { get; set; } = 1;
            public int Attack { get; set; } = 10;
            public int Defense { get; set; } = 5;
            public int HP { get; set; } = 100;
            public int Gold { get; set; } = 9999;
            public List<string> Inventory { get; set; } = new List<string>();
            public List<string> EquippedItem { get; set; } = new List<string>();

            public int BaseAttack = 10;
            public int BaseDefense = 5;

            
            public Player()
            {
                Attack = BaseAttack;
                Defense = BaseDefense;
            }

            public void Rest()
            {
                int RestCost = 500;
                int Heal = 50;
                int MaxHP = 100;

                if (Gold >= RestCost)
                {
                    Gold -= RestCost;
                    HP += Heal;
                    

                    if (HP > MaxHP)
                        HP = MaxHP;

                    Console.WriteLine("\n 휴식으로 체력이 회복되었습니다.");
                    Console.WriteLine($"현재 체력 : {HP} , 남은 골드 {Gold} G");
                }
                else
                {
                    Console.WriteLine("\n Gold가 충분하지 않습니다.");
                }
                
            }

            public void ShowStatus()
            {
                while (true)
                {
                    Console.WriteLine("\n==== [플레이어 상태] ====");
                    Console.WriteLine($"레벨: {Level}");
                    Console.WriteLine($"이름: {Name}");
                    Console.WriteLine($"직업: {Job}");
                    Console.WriteLine($"공격력: {Attack}");
                    Console.WriteLine($"방어력: {Defense}");
                    Console.WriteLine($"체력: {HP}");
                    Console.WriteLine($"Gold: {Gold} G");
                    Console.WriteLine("=========================");
                    Console.WriteLine("0. 나가기");
                    Console.Write("선택: ");

                    string input = Console.ReadLine();
                    if (input == "0") break;
                    else Console.WriteLine("잘못된 입력입니다. 다시 입력해주세요.");
                }
            }

            public void ShowInventory()
            {
                while (true)
                {
                    Console.WriteLine("\n==== [인벤토리] ====");
                    if (Inventory.Count == 0)
                        Console.WriteLine("인벤토리가 비어 있습니다.");
                    else
                    {
                        Console.WriteLine("[아이템 목록]");
                        foreach (var item in Inventory)
                        {
                            string status = (EquippedItem.Contains(item)) ? "[E] " : "";
                            Console.WriteLine($"- {status}{item}");
                        }
                    }
                    Console.WriteLine("=====================");
                    Console.WriteLine("1. 장착 관리");
                    Console.WriteLine("0. 나가기");
                    Console.Write("원하시는 행동을 입력해주세요: ");

                    string input = Console.ReadLine();
                    if (input == "0") break;
                    else if (input == "1") ManageEquipment();
                    else Console.WriteLine("잘못된 입력입니다. 다시 입력해주세요.");
                }
            }

            private void ManageEquipment()
            {
                if (Inventory.Count == 0)
                {
                    Console.WriteLine("장착할 아이템이 없습니다.");
                    return;
                }

                Console.WriteLine("\n==== [장착 관리] ====");

                int index = 1;

                foreach (var item in Inventory)
                {
                    string status = (EquippedItem.Contains(item) ? "[E] " : "");
                    string itemInfo = Shop.GetItemInfo(item);
                    Console.WriteLine($"{index}. {status}{item}-{itemInfo}");
                    
                    index++;
                }
                Console.WriteLine("0. 취소");

                Console.Write("장착/해제할 아이템 번호를 입력하세요: ");

                string input = Console.ReadLine();

                if (input == "0") return;

                if (int.TryParse(input, out int choice) && choice > 0 && choice <= Inventory.Count)
                {
                    string selectedItem = Inventory[choice - 1];
                    if (EquippedItem.Contains(selectedItem))
                    {
                        Console.WriteLine($"{selectedItem}을(를) 해제했습니다.");
                        EquippedItem.Remove(selectedItem);
                    }
                    else
                    {
                        EquippedItem.Add(selectedItem);
                        Console.WriteLine($"{selectedItem}을(를) 장착했습니다.");
                    }

                    UpdateStatus();
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                }
            }           
            private void UpdateStatus()
            {
                Attack = BaseAttack;
                Defense = BaseDefense;

                foreach (var item in EquippedItem)
                {
                    var stats = Shop.GetItemStats(item);
                    Attack += stats.Attack;
                    Defense += stats.Defense;
                }
            }
        }

        class Shop
        {
            private static Dictionary<string, (int Price, int Attack, int Defense, string Description)> items = new Dictionary<string, (int, int, int, string)>
        {
            { "수련자 갑옷", (1000,0,5, "방어력 +5 | 수련에 도움을 주는 갑옷입니다.") },
            { "무쇠 갑옷", (1500,0,9, "방어력 +9 | 무쇠로 만들어져 튼튼한 갑옷입니다.") },
            { "스파르타의 갑옷", (3500,0,15, "방어력 +15 | 스파르타의 전사들이 사용했다는 전설의 갑옷입니다.") },
            { "낡은 검", (600,2,0, "공격력 +2 | 쉽게 볼 수 있는 낡은 검 입니다.") },
            { "청동 도끼", (1500,5,0, "공격력 +5 | 어디선가 사용됐던거 같은 도끼입니다.") },
            { "스파르타의 창", (3500,7,0, "공격력 +7 | 스파르타의 전사들이 사용했다는 전설의 창입니다.") }
        };
            public static string GetItemInfo(string itemName)
            {
                if (items.ContainsKey(itemName))
                {
                    return items[itemName].Description; 
                }
                return "설명x";
            }
            

           
        
            public void OpenShop(Player player)
            {
                while (true)
                {
                    Console.WriteLine("\n==== [상점] ====");
                    Console.WriteLine($"보유 골드: {player.Gold} G");
                    Console.WriteLine("\n아이템 목록:");
                    int index = 1;

                    foreach (var item in items)
                    {
                        string status = player.Inventory.Contains(item.Key) ? "구매완료" : $"{item.Value.Price} G";
                        Console.WriteLine($"{index}. {item.Key} | {item.Value.Description} | {status}");
                        index++;
                    }


                    Console.WriteLine("\n0. 나가기");
                    Console.Write("구매할 아이템 번호를 입력하세요: ");
                    string input = Console.ReadLine();

                    if (input == "0")
                    {
                        Console.WriteLine("상점을 나갑니다.");
                        break;
                    }

                    if (int.TryParse(input, out int choice) && choice > 0 && choice <= items.Count)
                    {
                        string selectedItem = new List<string>(items.Keys)[choice - 1];
                        var itemData = items[selectedItem];

                        if (player.Inventory.Contains(selectedItem))
                        {
                            Console.WriteLine("이미 구매한 아이템입니다.");
                        }
                        else if (player.Gold >= itemData.Price)
                        {
                            player.Gold -= itemData.Price;
                            player.Inventory.Add(selectedItem);
                            Console.WriteLine($"{selectedItem}을(를) 구매했습니다!");
                            player.ShowInventory();
                        }
                        else
                        {
                            Console.WriteLine("Gold가 부족합니다.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("잘못된 입력입니다. 숫자를 입력하세요.");
                    }

                }
                
            }
            public static (int Attack, int Defense) GetItemStats(string itemName)
            {
                return items.ContainsKey(itemName) ? (items[itemName].Attack, items[itemName].Defense) : (0, 0);    
            }
            public static string GetItemDescription(string itemName)
            {
                return items.ContainsKey(itemName) ? items[itemName].Description : "설명X";
            }
        }
        
    }
}
