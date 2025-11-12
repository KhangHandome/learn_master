import os
import xml.etree.ElementTree as ET

input_dir = "./../ImageProcessing/annotations"   # thư mục chứa file .xml
output_dir = "./../ImageProcessing/labels"       # thư mục để lưu file .txt
os.makedirs(output_dir, exist_ok=True)

classes = ['licence']  # danh sách class

for file in os.listdir(input_dir):
    if not file.endswith('.xml'):
        continue
    tree = ET.parse(os.path.join(input_dir, file))
    root = tree.getroot()

    size = root.find('size')
    w = int(size.find('width').text)
    h = int(size.find('height').text)

    outpath = os.path.join(output_dir, file.replace('.xml', '.txt'))
    with open(outpath, 'w') as f:
        for obj in root.iter('object'):
            cls = obj.find('name').text
            if cls not in classes:
                continue
            cls_id = classes.index(cls)
            xmlbox = obj.find('bndbox')
            xmin = float(xmlbox.find('xmin').text)
            ymin = float(xmlbox.find('ymin').text)
            xmax = float(xmlbox.find('xmax').text)
            ymax = float(xmlbox.find('ymax').text)
            x_center = (xmin + xmax) / 2.0 / w
            y_center = (ymin + ymax) / 2.0 / h
            width = (xmax - xmin) / w
            height = (ymax - ymin) / h
            f.write(f"{cls_id} {x_center} {y_center} {width} {height}\n")

print("✅ Done converting VOC XML to YOLO TXT format!")
