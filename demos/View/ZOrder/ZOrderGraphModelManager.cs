/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using yWorks.Controls;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder {
  /// <summary>
  /// A <see cref="GraphModelManager"/> using <see cref="ZOrderSupport"/> as <see cref="ItemModelManager{T}.Comparer"/>
  /// for nodes.
  /// </summary>
  public class ZOrderGraphModelManager : GraphModelManager
  {
    private readonly ZOrderSupport ZOrderSupport;

    public ZOrderGraphModelManager(GraphControl graphControl, ZOrderSupport zOrderSupport) : base(graphControl, graphControl.ContentGroup) {
      ZOrderSupport = zOrderSupport;
      // The ItemModelManager.Comparer needs the user objects to be accessible from the main canvas objects
      ProvideUserObjectOnMainCanvasObject = true;
    }

    // Sets a z-Order aware comparer to the model manager used for nodes.
    protected override ItemModelManager<INode> CreateNodeModelManager(ICanvasObjectDescriptor descriptor, Func<INode, ICanvasObjectGroup> callback) {
      var nodeModelManager = base.CreateNodeModelManager(descriptor, callback);
      nodeModelManager.Comparer = ZOrderSupport;
      return nodeModelManager;
    }
  }
}
