// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace UIKit
// {
//     //依据 https://zhuanlan.zhihu.com/p/85846117 进行创建
//     //LRU 缓存算法的核心数据结构就是哈希链表，双向链表和哈希表的结合体
//     public class LRUCache<Key,Value>
//     {
//         //Node 类
//         internal class Node<Key,Value>
//         {
//             public Key key;
//             public Value value;
//             public Node<Key,Value> next;
//             public Node<Key,Value> prev;
//
//             public Node(Key k, Value v)
//             {
//                 this.key = k;
//                 value = v;
//             }
//         }
//         
//         //双链表
//         internal class DoubleLink<Key,Value>
//         {
//             //表头
//             private readonly Node<Key,Value> _head;
//             //节点个数
//             private int _count;
//             public int Count() => _count;
//             public bool IsEmpty() => (_count == 0);
//
//             public DoubleLink()
//             {
//                 _head = new Node<Key,Value>(default(Key), default(Value));//双向链表 表头为空
//                 _head.prev = _head;
//                 _head.next = _head;
//                 _count = 0;
//             }
//
//
//
//             //Append 追加到 index 之后
//             public void Add(int index, Key k, Value v)
//             {
//                 Node<Key, Value> inode;
//                 if (index == 0)
//                 {
//                     inode = _head;
//                 }
//                 else
//                 {
//                     index = index - 1;
//                     //获得当前 node 的前一个 node (逻辑上)
//                     inode = Query(index);
//                 }
//                 //tnode 当前创建的 node (逻辑上)
//                 var tnode = new Node<Key, Value>(k, v);
//                 
//                 //当前的node 的后一个 (逻辑上) 
//                 var pnode =  inode?.next;
//                 tnode.next = pnode;
//                 pnode.prev = tnode;
//                 
//                 //当前的node 的前一个是 inode
//                 inode.next = tnode;
//                 _count++;
//             }
//             
//             // 将节点插入到第index位置之前
//             public void Insert(int index, Key k, Value v)
//             {
//                 if (_count < 1 || index >= _count)
//                     throw new Exception("没有可插入的点或者索引溢出了");
//
//                 if (index == 0)
//                 {
//                     Add(index,k, v);
//                 }
//                 else
//                 {
//                     //后一个 node
//                     var inode = Query(index);
//                     //当前创建的 node
//                     var tnode = new Node<Key,Value>(k,v);
//                     //前一个 node
//                     var pnode = inode.prev;
//                     
//                     tnode.next = inode;
//                     inode.prev = tnode;
//                     
//                     tnode.prev = pnode;
//                     pnode.next = tnode;
//                     _count++;
//                 }
//             }
//
//             //删除
//             public Node<Key, Value> Remove(int index)
//             {
//                 var inode = Query(index);
//                 inode.prev.next = inode.next;
//                 inode.next.prev = inode.prev;
//                 _count--;
//                 return inode;
//             }
//             
//             public Node<Key, Value> RemoveFirst() => Remove(0);
//             public Node<Key, Value> RemoveLast() => Remove(_count - 1);
//
//             
//             public Node<Key, Value> Remove(Node<Key, Value> node)
//             {
//                 bool isHave = false;
//                 var inode = node;
//                 for (int i = 0; i < _count; i++)
//                 {
//                     inode = Query(i);
//                     if (!node.key.Equals(inode.key) || !node.value.Equals(inode.value)) continue;
//                     isHave = true;
//                     break;
//                 }
//
//                 if (!isHave) return default(Node<Key, Value>);
//                 inode.prev.next = inode.next;
//                 inode.next.prev = inode.prev;
//                 _count--;
//                 return inode;
//             }
//             
//             //查询
//             private Node<Key, Value> Query(int index)
//             {
//                 if (index < 0 || index >= _count)
//                     throw new IndexOutOfRangeException("索引溢出或者链表为空");
//                 
//                 if (index < _count / 2)//正向查找
//                 {
//                     var node = _head.next;
//                     for (int i = 0; i < index; i++)
//                         node = node.next;
//                     return node;
//                 }
//                 
//                 //反向查找
//                 var rnode = _head.prev;
//                 int rindex = _count - index - 1;
//                 for (int i = 0; i < rindex; i++)
//                     rnode = rnode.prev;
//                 return rnode;
//             }
//
//             public Key QueryKey(int index) => Query(index).key;
//             public Value QueryValue(int index) => Query(index).value;
//             public Key QueryFirstKey() => Query(0).key;
//             public Key QueryLastKey() => Query(_count - 1).key;
//             public Value QueryFirstValue() => Query(0).value;
//             public Value QueryLastValue() => Query(_count - 1).value;
//             
//             public void PrintAll()
//             {
//                 Debug.Log("******************* 链表数据如下 *******************");
//                 for (int i = 0; i < _count; i++)
//                 {
//                     var node = Query(i);
//                     Debug.Log("(" + i + ")=" + node.key + "     " + node.value);
//                 }
//                 Debug.Log("******************* 链表数据展示完毕 *******************");
//             }
//         }
//
//         
//         //key --> node 
//         private Dictionary<Key, Node<Key,Value>> _hashMap;
//         
//         private DoubleLink<Key,Value> _cache;
//         
//         // 最大容量
//         private int _capacity;
//
//         public LRUCache(int capacity)
//         {
//             _capacity = capacity;
//             _hashMap = new Dictionary<Key, Node<Key, Value>>();
//             _cache = new DoubleLink<Key, Value>();
//         }
//
//         public Value Query(Key k)
//         {
//             if (!_hashMap.TryGetValue(k, out Node<Key, Value> node)) return default(Value);
//             Update(node.key, node.value);
//             return node.value;
//         }
//
//         public void Update(Key k, Value v)
//         {
//             if (_hashMap.TryGetValue(k,out Node<Key,Value> node))
//             {
//                 _cache.Remove(node); //如果缓存中存在,就删除旧的节点,新的插到头部
//                 _cache.Insert(0,node.key,node.value); //将新的数据挪到开头 
//                 return;
//             }
//             if (_cache.Count() == _capacity) //装满了
//             {
//                 //删除链表最后一个数据
//                 var last = _cache.RemoveLast();
//                 _hashMap.Remove(last.key);
//             }
//             //将此数据插入头部
//             _cache.Insert(0,k,v);
//         }
//
//     }
// }