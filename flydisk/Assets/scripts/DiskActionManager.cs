using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseAction;
using My.Disk;
using UnityEngine.UI;

public class FlyAction : SSAction
{
    public Vector3 firstDirect;
    // 飞行方向
    private float time;
    // 已经飞行时间

    private static float g = 6f;
    public static FlyAction GetSSAction(Vector3 _firstDirect)
    {
        FlyAction currentAction = ScriptableObject.CreateInstance<FlyAction>();
        currentAction.firstDirect = _firstDirect;
        currentAction.time = 0;
        return currentAction;
    }

    public override void Start()
    {

    }

    public override void Update()
    {
        if (!this.gameObject.activeSelf) // 如果飞碟已经回收
        {
            this.destory = true;
            this.callback.SSEventAction(this, SSActionEventType.STARTED);
            return;
        }
        time += Time.deltaTime;
        transform.position += Time.deltaTime * firstDirect * Random.Range(1.5f,2.5f);
        // 各个方向的匀速运动
        transform.position += Vector3.down * g * time * Time.deltaTime;
        // 竖直方向的匀加速运动
        if (this.transform.position.y < -20)
        {
            this.destory = true;
            DiskFactory fac = Singleton<DiskFactory>.Instance;
            fac.freeDisk(gameObject);
            // 回收飞碟
            this.callback.SSEventAction(this);
        }
    }
}

public class DiskActionManager : SSActionManager, ISSActionCallback
{

    public GameObject cam;
    public Text centerText;

    DiskFactory dic;

    int sumNum;
    bool isLose;

    new void Start()
    {
        dic = Singleton<DiskFactory>.Instance;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            // 获取射线
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Disk")
            // 如果点击的物体是Disk
            {
                SceneController scene = Singleton<SceneController>.Instance;
                scene.recorder.AddScore(10); // 射中+10分
                
                dic.freeDisk(hit.collider.gameObject);
                // 点中，毁掉自身脚本，返回工厂
            }
        }
    }

    public void runActionByRound(int round)
    {
        sumNum = round;
        isLose = false;
        GameObject disk;
        for (int i = 0; i < round; i += 2)
        {
            // 随机产生发送点
            disk = dic.setDiskOnPos(new Vector3(Random.Range(-18f,-12f), Random.Range(10f,12f), Random.Range(5f,7f)));

            FlyAction fly = FlyAction.GetSSAction(new Vector3(Random.Range(5f, 15f), Random.Range(2.5f, 5f), Random.Range(0, 0.75f)));
            this.runAction(disk, fly, this);
        }

        for (int i = 1; i < round; i += 2)
        {
            disk = dic.setDiskOnPos(new Vector3(Random.Range(12f, 18f), Random.Range(10f, 12f), Random.Range(5f, 7f)));

            FlyAction fly = FlyAction.GetSSAction(new Vector3(Random.Range(-15f, -5f), Random.Range(2.5f, 5), Random.Range(0, 0.75f)));
            this.runAction(disk, fly, this);
        }
        // 设置飞碟发射，发射round个飞碟
    }

    // 回调函数
    public void SSEventAction(SSAction source, SSActionEventType events = SSActionEventType.COMPLETED,
        int intParam = 0, string strParam = null, Object objParam = null)
    {
        if (events == SSActionEventType.COMPLETED)
        // 落到y轴以下
        {
            isLose = true;
            sumNum--;
        }
        else
        {
            sumNum--;
        }

        if (sumNum == 0)
        // 如果本回合结束
        {
            SceneController scene = Singleton<SceneController>.Instance;
            if (isLose)
            {
                centerText.text = "LOSE";
                scene.nowState = Status.OVER;
            }
            else
            {
                RoundController round = Singleton<RoundController>.Instance;
                round.NextRound();
                scene.nowState = Status.WATING;
            }
        }
    }
}