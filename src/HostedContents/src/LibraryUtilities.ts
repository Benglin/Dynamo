
type MemberType = "none" | "creation" | "action" | "query";
type ElementType = "none" | "category" | "group";
type ItemType = "none" | "category" | "group" | "creation" | "action" | "query";

class TypeListNode {

    fullyQualifiedName: string = "";
    iconName: string = "";
    creationName: string = "";
    memberType: MemberType = "none";

    constructor(data: any) {
        this.fullyQualifiedName = data.creationName.split('@')[0]; // TODO: remove this hack
        this.iconName = data.iconName;
        this.creationName = data.creationName;
        this.memberType = data.itemType;
    }
}

class LayoutElement {

    text: string = "";
    iconName: string = "";
    elementType: ElementType = "none";
    include: string[] = [];
    childElements: LayoutElement[] = [];

    constructor(data: any) {
        this.text = data.text;
        this.iconName = data.iconName;
        this.elementType = data.elementType;
        this.include = data.include;
        if (data.childElements) {
            for (let i = 0; i < data.childElements.length; i++) {
                this.childElements.push(new LayoutElement(data.childElements[i])); 
            }
        }
    }
    appendChild(childElement: LayoutElement) {
        this.childElements.push(childElement);
    }
}

class LibraryItem {

    text: string = "";
    iconName: string = "";
    creationName: string = "";
    itemType: ItemType = "none";
    childItems: LibraryItem[] = [];

    constructor() {
    }

    constructMethod(listNode: TypeListNode) {
        this.text = listNode.fullyQualifiedName.split('.').pop();
        this.iconName = listNode.iconName;
        this.itemType = listNode.memberType;
    }

    constructClass(name: string) {
        this.text = name.split('.').pop();
        this.iconName = name;
        this.itemType = "none";
    }

    constructFromLayoutElement(layoutElement: LayoutElement) {
        this.text = layoutElement.text;
        this.iconName = layoutElement.iconName;
        this.itemType = layoutElement.elementType;
    }

    appendChild(childItem: LibraryItem) {
        this.childItems.push(childItem);
    }
}

/**
 * This method merges a type node (and its immediate sub nodes) under the given
 * library item.
 * 
 * Note that this is not a recursive function by design, it only considers the
 * current TypeTreeNode, and any possible child TypeTreeNode but nothing beyond
 * that depth.
 * 
 * @param {TypeTreeNode} typeTreeNode
 * The type node to be merged under the given library item. Its immediate child 
 * nodes will also be merged under the new library item, but the recursion does 
 * not go beyond that depth.
 * 
 * @param {LibraryItem} libraryItem
 * The library item under which a type node (and its sub nodes) is to be merged.
 */

function constructLibraryItem(
    typeListNodes: TypeListNode[],
    layoutElement: LayoutElement): LibraryItem
{
    let result = new LibraryItem();
    result.constructFromLayoutElement(layoutElement);

    // Traverse through the strings in 'include'
    for (let i = 0; i < layoutElement.include.length; i++) {
        let newClass = new LibraryItem();
        newClass.constructClass(layoutElement.include[i]);

        // Traverse through each node in typeListNodes (from RawTypeData.json)
        for (let j = 0; j < typeListNodes.length; j++) {

            // Check if the names contain the included strings
            if (typeListNodes[j].fullyQualifiedName.startsWith(layoutElement.include[i])) {
                let newMethod = new LibraryItem(); 
                newMethod.constructMethod(typeListNodes[j]);
                newClass.appendChild(newMethod);
            }
        }
        result.appendChild(newClass);
    }

    // Construct all child library items from child layout elements.
    for (let i = 0; i < layoutElement.childElements.length; i++) {
        let childLayoutElement = layoutElement.childElements[i];
        result.appendChild(constructLibraryItem(typeListNodes, childLayoutElement));
    }

    return result;
}

/**
 * Combine a data type tree and layout element tree to produce the resulting
 * library item tree.
 * 
 * @param {TypeTreeNode[]} typeTreeNodes
 * A tree of hierarchical data type identifiers. This tree is constructed 
 * based entirely on the loaded data types and their fully qualified names.
 * See TypeTreeNode for more information.
 * 
 * @param {LayoutElement[]} layoutElements
 * A tree serving as layout specifications from which library item tree is 
 * constructed. The specifications also contain information of how a given 
 * data type is merged into the resulting library item tree node.
 * 
 * @returns
 * Returns the resulting library item tree containing nodes merged from the 
 * type tree. The merging operation is done through the specifications of 
 * layout element tree.
 */
export function convertToLibraryTree(
    typeListNodes: TypeListNode[],
    layoutElements: LayoutElement[]): LibraryItem[]
{
    let results: LibraryItem[] = []; // Resulting tree of library items.

    // Generate the resulting library item tree before merging data types.
    for (let i = 0; i < layoutElements.length; i++) {

        let layoutElement = layoutElements[i];
        results.push(constructLibraryItem(typeListNodes, layoutElement));
    }

    return results;
}

export default function convertNow(loadedTypes: any, layoutSpecs: any): LibraryItem[]
{
    let typeListNodes: TypeListNode[] = [];
    let layoutElements: LayoutElement[] = [];

    // Converting raw data to strongly typed data.
    for (let i = 0; i < loadedTypes.loadedTypes.length; i++) {
        typeListNodes.push(new TypeListNode(loadedTypes.loadedTypes[i]));
    }

    for (let i = 0; i < layoutSpecs.elements.length; i++) {
        layoutElements.push(new LayoutElement(layoutSpecs.elements[i]));
    }
    return convertToLibraryTree(typeListNodes, layoutElements);
}
