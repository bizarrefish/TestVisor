
function Default(obj, propName, defaultValue)
{
	if(obj.hasOwnProperty(propName)) return obj[propName];
	else return defaultValue;
}

/*
function Html(tree)
{
	if(tree.constructor === String)
	{
		return tree;
	}
	
	var resultString = "";
	
	var nodeType = tree.type;
	var attr = Default(tree, "attr", {});
	var attrString = "";
	for(var name in attr)
	{
		var aValue = attr[name];
		attrString = attrString + " " + name + "=" + aValue;
	}
	
	resultString = resultString + "<" + nodeType + attrString + ">"
	
	var children = Default(tree, "children", []);
	for(var i in children)
	{
		resultString = resultString + Html(children[i])
	}
	
	resultString = resultString + "</" + nodeType + ">";
}

function Merge(p1, p2)
{
	var result = {};
	for(var i in p1)
	{
		result[i] = p1[i];
	}
	for(var i in p2)
	{
		result[i] = p2[i];
	}
	return result;
}

function Div(id, class, children)
{
	var attr = {}
	if(id !== null) attr.id = id;
	if(class !== null) attr.class = class;
	
	return {
		attr: attr,
		children: children
	}
}
*/

/*
Create HTML for div list:
props:
	id - list id
	title - list title
	desc - list description
*/
function Make_DivList(props)
{
	var title = Default(props, "title", "A list");
	var desc = Default(props, "desc", "This is a list");
	
	var id = Default(props, "id", "id");
	return '<div class="divList" id="' + id + '"></div>';
	return '<div class="divList" id="' + id + '">' +
	'<div class="heading">' + title + '</div>' +
	'<div class="desc">' + desc + '</div></div>';
}


function DivList_Clear(list)
{
	$(list).children("div.divListItem").remove();
}

/*
*/
function DivList_Add(list, id, title, description, icon, clickHandler)
{
	var imgCode = "";
	if(icon !== undefined && icon !== null) imgCode = '<img class="icon" src="' + icon + '">'

	var html = '<div class="divListItem" id="' + id + '">' + imgCode +
	'<div class="heading">' + title + '</div>' +
	'<div class="desc">' + description + '</div></div>'

	$(list).append(html);
	if(clickHandler !== undefined && clickHandler !== null)
	{
		$(list + " div#" + id).click(clickHandler);
		$(list + " div#" + id).addClass("clickable");
	}
}

function DivList_Remove(list, id)
{
	$(list).children("div.divListItem#" + id).remove();
}

function DivList_Select(list, id) {
	$(list).children("div.divListItem").removeClass("selected");
	$(list).children("div.divListItem#" + id).addClass("selected");
}
function DivList_GetElements(list)
{	
	
}
