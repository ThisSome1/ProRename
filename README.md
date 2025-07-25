Rename quick and easy, **multiple** **gameObjects** and **assets**.

You can use the bellow wildcards to dynamically rename your objects:
* `<name>` <br>
    This wildcard will be replaced by the old name of the object. <br>
    You can use range indexing to substring the old name. eg: `<name[1..^2]>`
* `<num>` <br>
    This wildcard will be replaced by an increasing number starting 1. <br>
    You can offset the number by + and -. eg: `<num+2>` or `<num-1>`
