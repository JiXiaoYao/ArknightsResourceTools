import cv2
import time
import numpy as np
import cupy as cp
from PIL import Image
import sys
import asyncio
import base64
import json
from concurrent.futures import ThreadPoolExecutor, wait, ALL_COMPLETED, FIRST_COMPLETED
import glob
import traceback
import os
import _thread as thread
import gc


BasePath = ""
End = False
Works = 0

def main():
    global End
    global BasePath,Timearray
    FileArray = []
    print('正在获取文件')
    T1 = time.time()
    if(len(sys.argv) > 1):
        if(os.path.isdir(sys.argv[1])):
            BasePath = sys.argv[1].replace("\"","").replace("\\","/")
            FileArray = SearchFile()
        else:
            BasePath = os.getcwd()
            FileArray = SearchFile()
    else:
        BasePath = os.getcwd()
        FileArray = SearchFile()
    if(len(FileArray) == 0):
        print("未发现可以处理的*.png与*[alpha].png")
        help()
        return
    if not os.path.exists(BasePath + "/OutPut"):
        os.makedirs(BasePath + "/OutPut")
    print("一共",len(FileArray),"个有效项目,",len(FileArray) * 2,"张原图")
    AllWork = len(FileArray)
    Bf = cp.ones((len(FileArray),256,1024))
    del Bf
    T2 = time.time()
    print("耗时:",round(T2 - T1,3),"秒")
    thread.start_new_thread(Count,(AllWork,))
    executor = ThreadPoolExecutor(max_workers=50)
    all_task = [executor.submit(Run, (value)) for value in FileArray]
    wait(all_task, return_when=ALL_COMPLETED)
    End = True
    T3 = time.time()
    print("阶段耗时:",round(T3 - T2,3),"秒")
    print("总耗时:",round(T3 - T1,3),"秒")
    print("均速:",round(((len(FileArray)) / (T3 - T2)),3),"张/每秒")
    print("实际成功：",len(os.listdir(BasePath + "/OutPut")))
    print("处理结束")
    input("回车退出....");

def Run(Dict):
        global BasePath
        global Works
        Works = Works + 1
        (RGB,Alpha) = Dict
        (RGB,Alpha,OutPut) = (BasePath + "/" + RGB,BasePath + "/" + Alpha,BasePath + "/OutPut/" + RGB)
        RGBImage = cp.array(cv2.imread(RGB,cv2.IMREAD_UNCHANGED))#cv2.IMREAD_UNCHANGED
        AlphaImage = cp.array(cv2.imread(Alpha))
        if(RGBImage.shape[0] > AlphaImage.shape[0]):
            AlphaImage = cp.kron(AlphaImage, cp.ones((2,2,1)))
        elif(RGBImage.shape[0] < AlphaImage.shape[0]):
            RGBImage = cp.kron(RGBImage, cp.ones((2,2,1)))
        NewAlpha = cp.ones((RGBImage.shape[0],RGBImage.shape[0],1))
        NewAlpha[:,:,0] = ((AlphaImage[:,:,0] + AlphaImage[:,:,1] + AlphaImage[:,:,2]) / 3)
        RGBImage[:,:,3:] = AlphaImage[:,:,:1]
        #im = Image.fromarray(cp.asnumpy(RGBImage))
        #im = Image.fromarray(RGBImage)
        #del RGBImage,AlphaImage,RGBImage
        #im.save(OutPut)
        cv2.imwrite(OutPut,cp.asnumpy(RGBImage))

def Count(AllWork):
    global End
    global Works
    while not End:
        print("已完成：",Works,"个项目 ",round((Works / AllWork) * 100),"%")
        time.sleep(2)

def SearchFile():
    global BasePath
    files = np.asanyarray(os.listdir(BasePath))
    FileArray = []
    for FileNameId in range(files.shape[0]):
        if "[alpha]" in files[FileNameId]:
            if (files[FileNameId].split("[alpha]")[0] + files[FileNameId].split("[alpha]")[1]) in files:
                FileArray.append([files[FileNameId].replace("[alpha]",""),str(files[FileNameId])])
        elif "_alpha" in files[FileNameId]:
            file = str(files[FileNameId])
            if (files[FileNameId].split("_alpha")[0] + files[FileNameId].split("_alpha")[1]) in files:
               FileArray.append([files[FileNameId].replace("_alpha",""),str(files[FileNameId])])
        elif "alpha" in files[FileNameId]:
            file = str(files[FileNameId])
            if (files[FileNameId].split("alpha")[0] + files[FileNameId].split("alpha")[1]) in files:
               FileArray.append([files[FileNameId].replace("alpha",""),str(files[FileNameId])])
    #FileArray = np.asanyarray(FileArray)
    return FileArray

def help():
        print("Windows下可以放在文件夹中直接运行")
        print("或者python ImgFusionCUDA.py [目录]")
        print("exe的话要运行: ImgFusionCPU.exe [directory]")
        print("Can be run directly in a folder")
        print("Or python ImgFusionCUDA.py [directory]")
        print("About exe need run: ImgFusionCPU.exe [directory]")

if(len(sys.argv) > 1):
    if(sys.argv[1] == "help"):
        help()
    elif(sys.argv[1] == "-h"):
        help()
    elif(sys.argv[1] == "--help"):
        help()
    else:
        print("程序已启动....")
        print("正在引导主函数")
        main()
else:
    print("程序已启动....")
    print("正在引导主函数")
    main()