using SPTarkov.Server.Core.Extensions;

namespace UnitTests.Tests.Extensions;

[TestClass]
public partial class ContainerExtensionsTests
{
    [TestInitialize]
    public void Initialize() { }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_1x1_Item_Fits_1x2_Container_At_0x0()
    {
        var container = new int[1, 2];
        var itemStartXPos = 0;
        var itemStartYPos = 0;
        var itemWidth = 1;
        var itemHeight = 1;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_1x1_Item_Fails_1x2_Container_At_0x0_With_Item_At_0x0()
    {
        var container = new int[1, 2];
        container[0, 0] = 1;
        var itemStartXPos = 0;
        var itemStartYPos = 0;
        var itemWidth = 1;
        var itemHeight = 1;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_1x2_Item_Fits_1x2_Container_At_0x0()
    {
        var container = new int[2, 1];
        var itemStartXPos = 0;
        var itemStartYPos = 0;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_1x2_Item_Fails_1x2_Container_At_0x0_With_Item_At_0x0()
    {
        var container = new int[1, 2];
        container[0, 0] = 1;
        var itemStartXPos = 0;
        var itemStartYPos = 0;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_2x2_Item_Fits_2x2_Container_At_0x0()
    {
        var container = new int[2, 2];
        var itemStartXPos = 0;
        var itemStartYPos = 0;
        var itemWidth = 2;
        var itemHeight = 2;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanItemBePlacedInContainerAtPosition_1x2_Item_Fits_2x2_Container_At_0x1()
    {
        var container = new int[2, 2];
        var itemStartXPos = 0;
        var itemStartYPos = 1;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.CanItemBePlacedInContainerAtPosition(
            itemStartXPos,
            itemStartYPos,
            itemWidth,
            itemHeight
        );

        Assert.IsTrue(result);
    }
}

public partial class ContainerExtensionsTests
{
    [TestMethod]
    public void FindSlotForItem_1x1_item_fits_1x1_container_no_rotation()
    {
        var container = new int[1, 1];
        var itemWidth = 1;
        var itemHeight = 1;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.Rotation);
        Assert.AreEqual(result.X, 0);
        Assert.AreEqual(result.Y, 0);
    }

    [TestMethod]
    public void FindSlotForItem_1x2_item_fits_3x3_container_rotated_with_items()
    {
        /// |1|1|1|
        /// |1|0|0|
        /// |1|1|1|
        var container = new int[3, 3];
        container[0, 0] = 1;
        container[0, 1] = 1;
        container[0, 2] = 1;
        container[1, 0] = 1;
        container[2, 0] = 1;
        container[2, 1] = 1;
        container[2, 2] = 1;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Rotation);
        Assert.AreEqual(result.X, 1);
        Assert.AreEqual(result.Y, 1);
    }

    [TestMethod]
    public void FindSlotForItem_1x1_item_fails_1x1_container_no_space()
    {
        var container = new int[1, 1];
        container[0, 0] = 1;
        var itemWidth = 1;
        var itemHeight = 1;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void FindSlotForItem_1x2_item_fits_1x2_container_no_rotation()
    {
        var container = new int[2, 1];
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.Rotation);
        Assert.AreEqual(result.X, 0);
        Assert.AreEqual(result.Y, 0);
    }

    [TestMethod]
    public void FindSlotForItem_1x2_item_fails_1x2_container_no_space()
    {
        var container = new int[1, 1];
        container[0, 0] = 1;
        container[0, 0] = 1;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void FindSlotForItem_2x2_item_fits_2x2_container_no_rotation()
    {
        var container = new int[2, 2];
        var itemWidth = 2;
        var itemHeight = 2;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.Rotation);
        Assert.AreEqual(result.X, 0);
        Assert.AreEqual(result.Y, 0);
    }

    [TestMethod]
    public void FindSlotForItem_1x2_item_fits_2x2_container_no_rotation_with_item_at_0x0()
    {
        var container = new int[2, 2];
        container[0, 0] = 1;
        var itemWidth = 1;
        var itemHeight = 2;

        var result = container.FindSlotForItem(itemWidth, itemHeight);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.Rotation);
        Assert.AreEqual(result.X, 1);
        Assert.AreEqual(result.Y, 0);
    }
}

public partial class ContainerExtensionsTests
{
    [TestMethod]
    public void FillContainerMapWithItem_1x1_at_0x0_in_1x1_no_rotation()
    {
        var container = new int[1, 1];

        var itemWidth = 1;
        var itemHeight = 1;

        var destinationPosX = 0;
        var destinationPosY = 0;
        var isRotated = false;

        container.FillContainerMapWithItem(
            destinationPosX,
            destinationPosY,
            itemWidth,
            itemHeight,
            isRotated
        );

        Assert.AreEqual(container[0, 0], 1);
    }

    [TestMethod]
    public void FillContainerMapWithItem_1x2_at_0x0_in_1x2_no_rotation()
    {
        var container = new int[2, 1];

        var itemWidth = 1;
        var itemHeight = 2;

        var destinationPosX = 0;
        var destinationPosY = 0;
        var isRotated = false;

        container.FillContainerMapWithItem(
            destinationPosX,
            destinationPosY,
            itemWidth,
            itemHeight,
            isRotated
        );

        Assert.AreEqual(container[0, 0], 1);
        Assert.AreEqual(container[1, 0], 1);
    }

    [TestMethod]
    public void FillContainerMapWithItem_2x2_at_0x0_in_2x2_no_rotation()
    {
        var container = new int[2, 2];

        var itemWidth = 2;
        var itemHeight = 2;

        var destinationPosX = 0;
        var destinationPosY = 0;
        var isRotated = false;

        container.FillContainerMapWithItem(
            destinationPosX,
            destinationPosY,
            itemWidth,
            itemHeight,
            isRotated
        );

        Assert.AreEqual(container[0, 0], 1);
        Assert.AreEqual(container[1, 1], 1);
    }

    [TestMethod]
    public void FillContainerMapWithItem_1x2_at_0x0_in_2x2_with_rotation()
    {
        var container = new int[2, 2];

        var itemWidth = 1;
        var itemHeight = 2;

        var destinationPosX = 0;
        var destinationPosY = 0;
        var isRotated = true;

        container.FillContainerMapWithItem(
            destinationPosX,
            destinationPosY,
            itemWidth,
            itemHeight,
            isRotated
        );

        Assert.AreEqual(container[0, 0], 1);
        Assert.AreEqual(container[0, 1], 1);
    }

    [TestMethod]
    public void FillContainerMapWithItem_1x2_at_1x0_in_2x2_with_rotation_with_existing_item()
    {
        var container = new int[2, 2];
        container[0, 0] = 1;

        var itemWidth = 1;
        var itemHeight = 2;

        var destinationPosX = 0;
        var destinationPosY = 1;
        var isRotated = true;

        container.FillContainerMapWithItem(
            destinationPosX,
            destinationPosY,
            itemWidth,
            itemHeight,
            isRotated
        );

        Assert.AreEqual(container[1, 0], 1);
        Assert.AreEqual(container[1, 1], 1);
    }
}
